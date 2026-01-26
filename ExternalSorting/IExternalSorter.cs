namespace ExternalSorting {
    public interface IExternalSorter {
        Task SortAsync(string filePath, string outputFilePath);
    }
}
