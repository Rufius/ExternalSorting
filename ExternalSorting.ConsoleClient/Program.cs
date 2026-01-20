using ExternalSorting.Generator;
using Microsoft.Extensions.Logging;

namespace ExternalSorting.ConsoleClient {
    internal class Program {
        static async Task Main(string[] args) {
            ILogger _logger = new CustomConsoleLogger();

            while (true) {
                var externalSorter = new ExternalSorter(Constants.ChunkSizeInBytes, Constants.ParallelChunksNumber);

                Console.Write("Choose action ([S] Sort, [G] Generate file, [Q] Quit): ");

                var actionKey = Console.ReadKey();
                Console.WriteLine();

                if (actionKey.KeyChar == 'S' || actionKey.KeyChar == 's') {
                    Console.WriteLine("Sorting is starting...");

                    await externalSorter.SortAsync("sample.txt");

                    Console.WriteLine("Sorting is complete.");

                } else if (actionKey.KeyChar == 'G' || actionKey.KeyChar == 'g') {
                    var lineGenerator = new RandomLineGenerator();
                    var duplicateTextLineProcessor = new DuplicateTextLineProcessor(4, 100, lineGenerator);
                    var generator = new FileGenerator(99999, duplicateTextLineProcessor, new CustomConsoleLogger());

                    Console.Write("Enter the name of the generated file [default - test.txt]: ");
                    string? fileName = Console.ReadLine();
                    fileName = string.IsNullOrWhiteSpace(fileName) ? "test.txt" : fileName;

                    Console.Write("Enter the size of the generated file in KB [default - 100 KB]: ");
                    string? sizeInKbString = Console.ReadLine();
                    sizeInKbString = string.IsNullOrWhiteSpace(sizeInKbString) ? "100" : sizeInKbString;
                    int sizeInKb = 0;

                    while (!int.TryParse(sizeInKbString, out sizeInKb)) {
                        Console.Write("The size of the file must be an integer. Enter again please: ");
                        sizeInKbString = Console.ReadLine();
                        sizeInKbString = string.IsNullOrWhiteSpace(sizeInKbString) ? "100" : sizeInKbString;
                    }

                    Console.WriteLine("Generation is starting...");

                    await generator.GenerateAsync(fileName, 1024 * sizeInKb);

                    Console.WriteLine("Generation is complete.");

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
