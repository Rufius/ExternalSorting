using ExternalSorting.Generator;
using Microsoft.Extensions.Logging;

namespace ExternalSorting {
    internal class Program {
        private static ILogger _logger = new CustomConsoleLogger();

        static async Task Main(string[] args) {
            while (true) {
                Console.Write("Choose action ([S] Sort, [G] Generate file, [Q] Quit): ");
                var actionKey = Console.ReadKey();
                Console.WriteLine();

                if (actionKey.KeyChar == 'S' || actionKey.KeyChar == 's') {
                    _logger.Log(LogLevel.Information, "Sorting is starting...");

                    var externalSorter = new ExternalSorter(Constants.ChunkSizeInBytes);
                    await externalSorter.SortAsync("sample.txt");
                } else if (actionKey.KeyChar == 'G' || actionKey.KeyChar == 'g') {
                    _logger.Log(LogLevel.Information, "Generation is starting...");

                    var generator = new FileGenerator(99999, new LineGenerator(), new CustomConsoleLogger());
                    await generator.Generate("test.txt", 1024);

                } else if (actionKey.KeyChar == 'Q' || actionKey.KeyChar == 'q') {
                    break;
                }
            }
        }
    }
}
