namespace Platform.Data.WebTerminal.Services
{
    public interface ITextFormatterService
    {
        string FormatErrorText(string errorText);
        string FormatJson(string jsonText);
        string FormatJsonWithStringFormatting(string jsonText);
    }
}