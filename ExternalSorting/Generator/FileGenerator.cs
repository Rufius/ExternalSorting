using Microsoft.Extensions.Logging;
using System.Text;

namespace ExternalSorting.Generator {
    public class FileGenerator {
        private readonly int NewlineBytesCount;
        private readonly int MaxNumber;
        const int NumberOfLinesPerBatch = 10;
        Random _random = new Random();

        private readonly ILineGenerator _lineGenerator;
        private readonly ILogger _logger;
       
        public FileGenerator(int maxNumber, ILineGenerator lineGenerator, ILogger logger) {
            MaxNumber = maxNumber;
            NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);
            _lineGenerator = lineGenerator;
            _logger = logger;
        }

        public async Task Generate(string filePath, int fileSize) {
            int currentSize = 0;
            using (var streamWriter = new StreamWriter(filePath)) {
                while (currentSize < fileSize) {

                    _logger?.Log(LogLevel.Information, "Generating next batch...");

                    // generate a batch of lines
                    List<string> textLines = new List<string>();
                    int numberOfDuplicates = 1;
                    for (int i = 0; i < NumberOfLinesPerBatch; i+=numberOfDuplicates) {
                        _logger?.Log(LogLevel.Information, $"Generating batch line #{i}");
                        string? textLine = await _lineGenerator.GenerateAsync();
                        numberOfDuplicates = _random.Next(5); // how many duplicates of this text will be in the file
                        for (int j = 0; j < numberOfDuplicates; j++) {
                            textLines.Add(textLine);
                        }
                    }

                    _logger?.Log(LogLevel.Information, "Batch is generated.");

                    _logger?.Log(LogLevel.Information, "Writing batch lines to the output file...");

                    // write batch lines to the output file
                    foreach (string textLine in textLines) {
                        int number = _random.Next(MaxNumber);
                        string line = $"{number}. {textLine}";
                        await streamWriter.WriteLineAsync(line);
                        currentSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;
                    }

                    _logger?.Log(LogLevel.Information, "Batch is written to the output file");
                }
            }
        }

        private async Task<string> GenerateLineAsync() {
            int number = _random.Next(MaxNumber);
            string? text = await _lineGenerator.GenerateAsync();

            return $"{number}. {text}";
        }
    }
}
