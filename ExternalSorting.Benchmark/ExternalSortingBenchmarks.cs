using BenchmarkDotNet.Attributes;

namespace ExternalSorting.Benchmark {
    [MemoryDiagnoser]
    [ShortRunJob]
    public class ExternalSortingBenchmarks {
        private const int ChunkSizeInBytes = 100;
        private const int ParallelChunksNumber = 10;

        private readonly ExternalSorter _externalSorter = new ExternalSorter(ChunkSizeInBytes, ParallelChunksNumber, null);

        [Benchmark]
        public async Task SortSmallFile() {
            await _externalSorter.SortAsync("sample.txt", "output.txt");
        }
    }
}
