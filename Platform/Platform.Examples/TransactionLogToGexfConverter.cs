using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Xml;

namespace Platform.Examples
{
    public class TransactionLogToGexfConverter
    {
        private enum TransactionItemType
        {
            Creation,
            UpdateOf,
            UpdateTo,
            Deletion
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LinkStruct
        {
            public long Address;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransactionItem
        {
            public long TransactionId;
            public long DateTimeTicks;
            public TransactionItemType Type;
            public LinkStruct Source;
            public LinkStruct Linker;
            public LinkStruct Target;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransactionsState
        {
            public long LastTransactionId;
            public long LastTransactionOffset;
            public long LastTransactionItemsCount;
            public long LastHandledTransactionId;
            public long LastHandledTransactionOffset;
            public long LastHandledTransactionItemOffset;
            public long FileEndOffset;
            public long FileSizeInTransactionItems;
        }

        private readonly long _basicTransactionsOffset = Marshal.SizeOf<TransactionsState>();
        private readonly long _transactionItemSize = Marshal.SizeOf<TransactionItem>();
        private readonly string _transactionLogFile;

        public TransactionLogToGexfConverter(string transactionLogFile)
        {
            _transactionLogFile = transactionLogFile;
        }

        public void Export(string outputFilename)
        {
            if (!File.Exists(_transactionLogFile))
            {
                throw new FileNotFoundException($"Transaction log file not found: {_transactionLogFile}");
            }

            var transactions = ReadTransactions();
            WriteGexfDynamicGraph(outputFilename, transactions);
        }

        private List<TransactionItem> ReadTransactions()
        {
            var transactions = new List<TransactionItem>();
            var fileInfo = new FileInfo(_transactionLogFile);
            var fileSize = fileInfo.Length;

            if (fileSize < _basicTransactionsOffset)
            {
                return transactions;
            }

            using (var mmf = MemoryMappedFile.CreateFromFile(_transactionLogFile, FileMode.Open))
            using (var accessor = mmf.CreateViewAccessor())
            {
                accessor.Read(0, out TransactionsState state);

                if (state.FileEndOffset <= _basicTransactionsOffset)
                {
                    return transactions;
                }

                var itemsCount = (state.FileEndOffset - _basicTransactionsOffset) / _transactionItemSize;

                for (long i = 0; i < itemsCount; i++)
                {
                    var offset = _basicTransactionsOffset + i * _transactionItemSize;
                    accessor.Read(offset, out TransactionItem item);
                    transactions.Add(item);
                }
            }

            return transactions;
        }

        private void WriteGexfDynamicGraph(string outputFilename, List<TransactionItem> transactions)
        {
            var nodes = new HashSet<long>();
            var nodeSpells = new Dictionary<long, List<(DateTime start, DateTime? end, TransactionItemType type)>>();
            var edges = new List<(long id, long source, long target, DateTime timestamp, TransactionItemType type)>();
            var edgeId = 0L;

            foreach (var transaction in transactions)
            {
                var dateTime = new DateTime(transaction.DateTimeTicks);
                var sourceId = transaction.Source.Address;
                var linkerId = transaction.Linker.Address;
                var targetId = transaction.Target.Address;

                nodes.Add(sourceId);
                nodes.Add(linkerId);
                nodes.Add(targetId);

                if (!nodeSpells.ContainsKey(sourceId))
                {
                    nodeSpells[sourceId] = new List<(DateTime, DateTime?, TransactionItemType)>();
                }

                switch (transaction.Type)
                {
                    case TransactionItemType.Creation:
                        nodeSpells[sourceId].Add((dateTime, null, transaction.Type));
                        edges.Add((edgeId++, sourceId, targetId, dateTime, transaction.Type));
                        break;
                    case TransactionItemType.Deletion:
                        if (nodeSpells[sourceId].Count > 0)
                        {
                            var lastSpell = nodeSpells[sourceId][nodeSpells[sourceId].Count - 1];
                            if (lastSpell.end == null)
                            {
                                nodeSpells[sourceId][nodeSpells[sourceId].Count - 1] = (lastSpell.start, dateTime, lastSpell.type);
                            }
                        }
                        break;
                    case TransactionItemType.UpdateOf:
                    case TransactionItemType.UpdateTo:
                        edges.Add((edgeId++, sourceId, targetId, dateTime, transaction.Type));
                        break;
                }
            }

            using (var file = File.OpenWrite(outputFilename))
            using (var writer = XmlWriter.Create(file, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("gexf", "http://gexf.net/1.3");
                writer.WriteAttributeString("version", "1.3");

                writer.WriteStartElement("meta");
                writer.WriteAttributeString("lastmodifieddate", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                writer.WriteStartElement("creator");
                writer.WriteString("LinksPlatform TransactionLogToGexfConverter");
                writer.WriteEndElement();
                writer.WriteStartElement("description");
                writer.WriteString("Dynamic graph from transaction log");
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("graph");
                writer.WriteAttributeString("mode", "dynamic");
                writer.WriteAttributeString("defaultedgetype", "directed");
                writer.WriteAttributeString("timeformat", "dateTime");

                writer.WriteStartElement("attributes");
                writer.WriteAttributeString("class", "node");
                writer.WriteAttributeString("mode", "static");
                writer.WriteStartElement("attribute");
                writer.WriteAttributeString("id", "0");
                writer.WriteAttributeString("title", "type");
                writer.WriteAttributeString("type", "string");
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("attributes");
                writer.WriteAttributeString("class", "edge");
                writer.WriteAttributeString("mode", "static");
                writer.WriteStartElement("attribute");
                writer.WriteAttributeString("id", "0");
                writer.WriteAttributeString("title", "transactionType");
                writer.WriteAttributeString("type", "string");
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("nodes");
                foreach (var nodeId in nodes)
                {
                    writer.WriteStartElement("node");
                    writer.WriteAttributeString("id", nodeId.ToString());
                    writer.WriteAttributeString("label", $"Link {nodeId}");

                    if (nodeSpells.ContainsKey(nodeId) && nodeSpells[nodeId].Count > 0)
                    {
                        writer.WriteStartElement("spells");
                        foreach (var spell in nodeSpells[nodeId])
                        {
                            writer.WriteStartElement("spell");
                            writer.WriteAttributeString("start", spell.start.ToString("yyyy-MM-ddTHH:mm:ss"));
                            if (spell.end.HasValue)
                            {
                                writer.WriteAttributeString("end", spell.end.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteStartElement("attvalues");
                    writer.WriteStartElement("attvalue");
                    writer.WriteAttributeString("for", "0");
                    writer.WriteAttributeString("value", "link");
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("edges");
                foreach (var edge in edges)
                {
                    writer.WriteStartElement("edge");
                    writer.WriteAttributeString("id", edge.id.ToString());
                    writer.WriteAttributeString("source", edge.source.ToString());
                    writer.WriteAttributeString("target", edge.target.ToString());
                    writer.WriteAttributeString("start", edge.timestamp.ToString("yyyy-MM-ddTHH:mm:ss"));

                    writer.WriteStartElement("attvalues");
                    writer.WriteStartElement("attvalue");
                    writer.WriteAttributeString("for", "0");
                    writer.WriteAttributeString("value", edge.type.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
