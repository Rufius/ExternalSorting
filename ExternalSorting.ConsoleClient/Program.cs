using ExternalSorting.Generator;
using System.Diagnostics;

namespace ExternalSorting.ConsoleClient {
    internal class Program {
        static async Task Main(string[] args) {
            // setup external sorter
            var logger = new CustomConsoleLogger();
            var externalSorter = new ExternalSorter(Constants.ChunkSizeInBytes, Constants.ParallelChunksNumber, logger);

            // setup file generator
            var lineGenerator = new RandomLineGenerator();
            var duplicateTextLineProcessor = new DuplicateTextLineProcessor(4, 100, lineGenerator);
            var generator = new FileGenerator(99999, duplicateTextLineProcessor, logger);

            var stopwatch = new Stopwatch();

            while (true) {
                Console.Write("Choose action ([S] Sort, [G] Generate file, [Q] Quit): ");

                var actionKey = Console.ReadKey();
                Console.WriteLine();

                if (actionKey.KeyChar == 'S' || actionKey.KeyChar == 's') {
                    Console.Write("Enter the path to the input file [default - sample.txt]: ");
                    string? inputFilePath = Console.ReadLine();
                    inputFilePath = string.IsNullOrWhiteSpace(inputFilePath) ? "sample.txt" : inputFilePath;

                    Console.Write("Enter the path to the file [default - output.txt]: ");
                    string? outputFilePath = Console.ReadLine();
                    outputFilePath = string.IsNullOrWhiteSpace(outputFilePath) ? "output.txt" : outputFilePath;

                    Console.Write("Enter the chunk size in bytes [default - 100 bytes]: ");
                    string? chunkSizeString = Console.ReadLine();
                    chunkSizeString = string.IsNullOrWhiteSpace(chunkSizeString) ? "100" : chunkSizeString;
                    int chunkSize = 0;

                    while (!int.TryParse(chunkSizeString, out chunkSize)) {
                        Console.Write("The chunk size must be an integer. Enter again please [default - 100 bytes]: ");
                        chunkSizeString = Console.ReadLine();
                        chunkSizeString = string.IsNullOrWhiteSpace(chunkSizeString) ? "100" : chunkSizeString;
                    }

                    externalSorter.ChunkSizeInBytes = chunkSize;

                    Console.Write("Enter the number of chunks processed in parallel [default - 10]: ");
                    string? numberOfChunksString = Console.ReadLine();
                    numberOfChunksString = string.IsNullOrWhiteSpace(numberOfChunksString) ? "10" : numberOfChunksString;
                    int numberOfChunks = 0;

                    while (!int.TryParse(numberOfChunksString, out numberOfChunks)) {
                        Console.Write("The number of chunks must be an integer. Enter again please [default - 10]: ");
                        numberOfChunksString = Console.ReadLine();
                        numberOfChunksString = string.IsNullOrWhiteSpace(numberOfChunksString) ? "10" : numberOfChunksString;
                    }

                    externalSorter.ParallelChunksNumber = numberOfChunks;

                    Console.WriteLine("Sorting is starting...");
                    stopwatch.Restart();
                    await externalSorter.SortAsync(inputFilePath, outputFilePath);
                    stopwatch.Stop();
                    Console.WriteLine("Sorting is completed.");
                    Console.WriteLine("Time elapsed: " + stopwatch.Elapsed.ToString());

                } else if (actionKey.KeyChar == 'G' || actionKey.KeyChar == 'g') {
                    Console.Write("Enter the name of the generated file [default - test.txt]: ");
                    string? fileName = Console.ReadLine();
                    fileName = string.IsNullOrWhiteSpace(fileName) ? "test.txt" : fileName;

                    Console.Write("Enter the size of the generated file in KB [default - 100 KB]: ");
                    string? sizeInKbString = Console.ReadLine();
                    sizeInKbString = string.IsNullOrWhiteSpace(sizeInKbString) ? "100" : sizeInKbString;
                    int sizeInKb = 0;

                    while (!int.TryParse(sizeInKbString, out sizeInKb)) {
                        Console.Write("The size of the file must be an integer. Enter again please [default - 100 KB]: ");
                        sizeInKbString = Console.ReadLine();
                        sizeInKbString = string.IsNullOrWhiteSpace(sizeInKbString) ? "100" : sizeInKbString;
                    }

                    Console.WriteLine("Generation is starting...");

                    await generator.GenerateAsync(fileName, 1024 * sizeInKb);

                    Console.WriteLine("Generation is complete.");

                } else if (actionKey.KeyChar == 'Q' || actionKey.KeyChar == 'q') {
                    break;
                } else {
                    Console.WriteLine("Wrong action. Please choose again.");
                }

                Console.WriteLine();
            }
        }
    }
}
