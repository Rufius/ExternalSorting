namespace ExternalSorting {
    internal class Program {
        static async Task Main(string[] args) {
            var externalSorter = new ExternalSorter(Constants.ChunkSize);
            await externalSorter.SortAsync("sample.txt");
        }
    }
}
