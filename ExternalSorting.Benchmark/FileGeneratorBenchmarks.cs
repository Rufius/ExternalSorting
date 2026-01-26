using BenchmarkDotNet.Attributes;
using ExternalSorting.Generator;

namespace ExternalSorting.Benchmark {
    [MemoryDiagnoser]
    [ShortRunJob]
    [InProcess]
    public class FileGeneratorBenchmarks {
        private const int MaxNumberOfTextDuplicates = 99999;
        private const int MaxNumberOfIterationsBeforeDuplicates = 100;
        private const int MaxNumber = 99999;

        private FileGenerator _fileGenerator;

        [GlobalSetup]
        public void Setup() {
            var lineGenerator = new RandomLineGenerator();
            var duplicateTextLineProcessor = new DuplicateTextLineProcessor(MaxNumberOfTextDuplicates, 100, lineGenerator);
            var _fileGenerator = new FileGenerator(MaxNumber, duplicateTextLineProcessor, null);
        }

        [Benchmark]
        public async Task GenerateSmallFile() {
            await _fileGenerator.GenerateAsync("testfile.txt", 1024 * 5);
        }
    }
}
