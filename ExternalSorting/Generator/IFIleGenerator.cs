namespace ExternalSorting.Generator {
    internal interface IFileGenerator {
        Task GenerateAsync(string filePath, int fileSize);
    }
}
