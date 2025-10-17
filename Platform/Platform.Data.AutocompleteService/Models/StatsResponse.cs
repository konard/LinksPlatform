namespace Platform.Data.AutocompleteService.Models
{
    public class StatsResponse
    {
        public ulong TotalLinks { get; set; }
        public ulong IndexedSequences { get; set; }
        public int UniqueSequences { get; set; }
    }
}
