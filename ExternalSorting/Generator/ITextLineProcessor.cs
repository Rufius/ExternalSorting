namespace ExternalSorting.Generator {
    public interface ITextLineProcessor {
        Task<string> GetTextLineAsync();
    }
}
