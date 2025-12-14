namespace ExternalSorting.Generator {
    public interface ILineGenerator {
        Task<string?> GenerateAsync();
    }
}
