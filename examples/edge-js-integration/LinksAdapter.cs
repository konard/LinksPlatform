using System;
using System.Linq;
using System.Threading.Tasks;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Disposables;

namespace EdgeJsIntegration
{
    public class LinksAdapter
    {
        private UnitedMemoryLinks<ulong> _links;

        public LinksAdapter(string dataFilePath)
        {
            var dataMemory = new FileMappedResizableDirectMemory(dataFilePath);
            _links = new UnitedMemoryLinks<ulong>(dataMemory);
        }

        public Task<object> CreateLink(dynamic input)
        {
            try
            {
                var source = (ulong)input.source;
                var target = (ulong)input.target;
                var link = _links.Create();
                _links.Update(link, source, target);
                return Task.FromResult<object>(new { success = true, link = link });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }

        public Task<object> GetOrCreate(dynamic input)
        {
            try
            {
                var source = (ulong)input.source;
                var target = (ulong)input.target;
                var link = _links.GetOrCreate(source, target);
                return Task.FromResult<object>(new { success = true, link = link });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }

        public Task<object> Update(dynamic input)
        {
            try
            {
                var link = (ulong)input.link;
                var newSource = (ulong)input.newSource;
                var newTarget = (ulong)input.newTarget;
                _links.Update(link, newSource, newTarget);
                return Task.FromResult<object>(new { success = true });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }

        public Task<object> Delete(dynamic input)
        {
            try
            {
                var link = (ulong)input.link;
                _links.Delete(link);
                return Task.FromResult<object>(new { success = true });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }

        public Task<object> GetLink(dynamic input)
        {
            try
            {
                var link = (ulong)input.link;
                ulong[] linkData = new ulong[3];
                _links.Each((readLink) =>
                {
                    linkData[0] = readLink[0]; // index
                    linkData[1] = readLink[1]; // source
                    linkData[2] = readLink[2]; // target
                    return _links.Constants.Break;
                }, new Link<ulong>(link, _links.Constants.Any, _links.Constants.Any));

                return Task.FromResult<object>(new
                {
                    success = true,
                    index = linkData[0],
                    source = linkData[1],
                    target = linkData[2]
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }

        public Task<object> Count(dynamic input)
        {
            try
            {
                var count = _links.Count(new Link<ulong>(_links.Constants.Any, _links.Constants.Any, _links.Constants.Any));
                return Task.FromResult<object>(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, error = ex.Message });
            }
        }

        public void Dispose()
        {
            _links?.DisposeIfPossible();
        }
    }
}
