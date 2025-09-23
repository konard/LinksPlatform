using Microsoft.AspNetCore.Mvc;
using Platform.Data.WebTerminal.Services;

namespace Platform.Data.WebTerminal.Controllers
{
    public class FormatterController : Controller
    {
        private readonly ITextFormatterService _textFormatterService;

        public FormatterController(ITextFormatterService textFormatterService)
        {
            _textFormatterService = textFormatterService;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult FormatText(string inputText, string formatType)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                ViewBag.FormattedText = "";
                ViewBag.InputText = "";
                ViewBag.FormatType = formatType;
                return View("Index");
            }

            string formattedText;
            try
            {
                switch (formatType?.ToLower())
                {
                    case "error":
                        formattedText = _textFormatterService.FormatErrorText(inputText);
                        break;
                    case "json":
                        formattedText = _textFormatterService.FormatJson(inputText);
                        break;
                    case "jsonstrings":
                        formattedText = _textFormatterService.FormatJsonWithStringFormatting(inputText);
                        break;
                    default:
                        formattedText = _textFormatterService.FormatErrorText(inputText);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                formattedText = $"Error during formatting: {ex.Message}";
            }

            ViewBag.FormattedText = formattedText;
            ViewBag.InputText = inputText;
            ViewBag.FormatType = formatType;
            return View("Index");
        }
    }
}