using Microsoft.AspNetCore.Mvc;
using Platform.Services.WordValidator.Models;
using Platform.Services.WordValidator.Services;

namespace Platform.Services.WordValidator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly IWordValidationService _wordValidationService;

        public WordController(IWordValidationService wordValidationService)
        {
            _wordValidationService = wordValidationService;
        }

        /// <summary>
        /// Checks if the provided text is a valid word
        /// </summary>
        /// <param name="word">The word to validate</param>
        /// <returns>WordValidationResponse indicating if the input is a word</returns>
        [HttpGet("validate/{word}")]
        public ActionResult<WordValidationResponse> ValidateWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest(new { error = "Word parameter cannot be empty" });
            }

            var isWord = _wordValidationService.IsWord(word);
            return Ok(new WordValidationResponse
            {
                Word = word,
                IsWord = isWord
            });
        }

        /// <summary>
        /// Checks if the provided text is a valid word (via query parameter)
        /// </summary>
        /// <param name="word">The word to validate</param>
        /// <returns>WordValidationResponse indicating if the input is a word</returns>
        [HttpGet("validate")]
        public ActionResult<WordValidationResponse> ValidateWordQuery([FromQuery] string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest(new { error = "Word parameter cannot be empty" });
            }

            var isWord = _wordValidationService.IsWord(word);
            return Ok(new WordValidationResponse
            {
                Word = word,
                IsWord = isWord
            });
        }
    }
}
