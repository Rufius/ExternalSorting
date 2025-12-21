using ExternalSorting.Generator;
using Microsoft.Extensions.Logging;

namespace ExternalSorting {
    internal class Program {
        private static ILogger _logger = new CustomConsoleLogger();

        static async Task Main(string[] args) {
            var externalSorter = new ExternalSorter(Constants.ChunkSizeInBytes);
            var generator = new FileGenerator(99999, new RandomLineGenerator(), new CustomConsoleLogger());

            while (true) {
                Console.Write("Choose action ([S] Sort, [G] Generate file, [Q] Quit): ");

                var actionKey = Console.ReadKey();
                Console.WriteLine();

                if (actionKey.KeyChar == 'S' || actionKey.KeyChar == 's') {
                    _logger.Log(LogLevel.Information, "Sorting is starting...");

                    await externalSorter.SortAsync("sample.txt");

                    _logger.Log(LogLevel.Information, "Sorting is complete.");

                } else if (actionKey.KeyChar == 'G' || actionKey.KeyChar == 'g') {
                    _logger.Log(LogLevel.Information, "Generation is starting...");

                    await generator.Generate("test.txt", 1024 * 1024 * 500);

                    _logger.Log(LogLevel.Information, "Generation is complete.");

                } else if (actionKey.KeyChar == 'Q' || actionKey.KeyChar == 'q') {
                    break;
                } else {
                    Console.WriteLine("Wrong action. Please choose again");
                }

                Console.WriteLine();
            }
        }
    }
}
