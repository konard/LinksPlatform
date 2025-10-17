using System.Collections.Generic;
using Platform.Numbers;
using Platform.Data.Numbers.Raw;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    public class LinksCodeStorage<TLink> : ICodeStorage<TLink>
    {
        private static readonly TLink _zero = default;
        private static readonly TLink _one = Arithmetic.Increment(_zero);

        private readonly StringToUnicodeSequenceConverter<TLink> _stringToUnicodeSequenceConverter;
        private readonly ILinks<TLink> _links;
        private TLink _unicodeSymbolMarker;
        private TLink _unicodeSequenceMarker;
        private TLink _repositoryMarker;
        private TLink _fileMarker;
        private TLink _directoryMarker;
        private TLink _commitMarker;
        private TLink _filePathMarker;
        private TLink _fileContentMarker;
        private TLink _commitMessageMarker;
        private TLink _commitAuthorMarker;

        private class Unindex : ISequenceIndex<TLink>
        {
            public bool Add(IList<TLink> sequence) => true;
            public bool MightContain(IList<TLink> sequence) => true;
        }

        public LinksCodeStorage(ILinks<TLink> links, bool indexSequenceBeforeCreation, LinkFrequenciesCache<TLink> frequenciesCache)
        {
            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<TLink>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<TLink>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<TLink>(links, sequenceToItsLocalElementLevelsConverter);
            InitConstants(links);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(links, new AddressToRawNumberConverter<TLink>(), _unicodeSymbolMarker);
            var index = indexSequenceBeforeCreation ? new CachedFrequencyIncrementingSequenceIndex<TLink>(frequenciesCache) : (ISequenceIndex<TLink>)new Unindex();
            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<TLink>(links, charToUnicodeSymbolConverter, index, optimalVariantConverter, _unicodeSequenceMarker);
            _links = links;
        }

        private void InitConstants(ILinks<TLink> links)
        {
            var markerIndex = _one;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _repositoryMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _fileMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _directoryMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _commitMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _filePathMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _fileContentMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _commitMessageMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _commitAuthorMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
        }

        public TLink CreateRepository(string name) => Create(_repositoryMarker, name);

        public TLink CreateFile(string path, string content)
        {
            var pathSequence = _stringToUnicodeSequenceConverter.Convert(path);
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(content);
            var pathLink = _links.GetOrCreate(_filePathMarker, pathSequence);
            var contentLink = _links.GetOrCreate(_fileContentMarker, contentSequence);
            var file = _links.GetOrCreate(_fileMarker, pathLink);
            _links.GetOrCreate(file, contentLink);
            return file;
        }

        public TLink CreateDirectory(string path) => Create(_directoryMarker, path);

        public TLink CreateCommit(string message, string author, TLink parent)
        {
            var messageSequence = _stringToUnicodeSequenceConverter.Convert(message);
            var authorSequence = _stringToUnicodeSequenceConverter.Convert(author);
            var messageLink = _links.GetOrCreate(_commitMessageMarker, messageSequence);
            var authorLink = _links.GetOrCreate(_commitAuthorMarker, authorSequence);
            var commit = _links.GetOrCreate(_commitMarker, messageLink);
            _links.GetOrCreate(commit, authorLink);
            if (!EqualityComparer<TLink>.Default.Equals(parent, _zero))
            {
                _links.GetOrCreate(commit, parent);
            }
            return commit;
        }

        private TLink Create(TLink marker, string content)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(content);
            return _links.GetOrCreate(marker, contentSequence);
        }

        public void AttachFileToDirectory(TLink file, TLink directory) => _links.GetOrCreate(directory, file);

        public void AttachCommitToRepository(TLink commit, TLink repository) => _links.GetOrCreate(repository, commit);

        public void AttachFileToCommit(TLink file, TLink commit) => _links.GetOrCreate(commit, file);
    }
}
