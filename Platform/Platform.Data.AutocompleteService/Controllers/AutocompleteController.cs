using Microsoft.AspNetCore.Mvc;
using Platform.Data.AutocompleteService.Models;
using Platform.Data.AutocompleteService.Services;

namespace Platform.Data.AutocompleteService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutocompleteController : ControllerBase
    {
        private readonly AutocompleteEngine _engine;

        public AutocompleteController(AutocompleteEngine engine)
        {
            _engine = engine;
        }

        [HttpPost("suggest")]
        public ActionResult<AutocompleteResponse> GetSuggestions([FromBody] AutocompleteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prefix))
            {
                return BadRequest(new { error = "Prefix is required" });
            }

            var suggestions = _engine.GetSuggestions(request.Prefix, request.MaxResults);

            return Ok(new AutocompleteResponse
            {
                Prefix = request.Prefix,
                Suggestions = suggestions,
                Count = suggestions.Count
            });
        }

        [HttpGet("suggest")]
        public ActionResult<AutocompleteResponse> GetSuggestionsGet([FromQuery] string prefix, [FromQuery] int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return BadRequest(new { error = "Prefix is required" });
            }

            var suggestions = _engine.GetSuggestions(prefix, maxResults);

            return Ok(new AutocompleteResponse
            {
                Prefix = prefix,
                Suggestions = suggestions,
                Count = suggestions.Count
            });
        }

        [HttpPost("index")]
        public ActionResult IndexText([FromBody] IndexTextRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { error = "Text is required" });
            }

            _engine.IndexText(request.Text);

            return Ok(new { message = "Text indexed successfully" });
        }

        [HttpGet("stats")]
        public ActionResult<StatsResponse> GetStats()
        {
            return Ok(new StatsResponse
            {
                IndexedSequences = _engine.GetIndexedSequencesCount(),
                UniqueSequences = _engine.GetUniqueSequencesCount()
            });
        }
    }
}
