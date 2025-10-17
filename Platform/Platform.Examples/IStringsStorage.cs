namespace Platform.Examples
{
    public interface IStringsStorage<TLink>
    {
        TLink Store(string @string);
        string Get(TLink link);
        bool Contains(string @string);
        TLink GetOrCreate(string @string);
    }
}
