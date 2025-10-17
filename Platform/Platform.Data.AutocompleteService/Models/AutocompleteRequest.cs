namespace Platform.Data.AutocompleteService.Models
{
    public class AutocompleteRequest
    {
        public string Prefix { get; set; }
        public int MaxResults { get; set; } = 10;
    }
}
