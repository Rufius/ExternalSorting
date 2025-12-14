using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Generator {
    public class FileGenerator {
        private readonly int NewlineBytesCount;
        private readonly int MaxNumber;
        const int BatchSize = 10;
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
                    List<string> lines = new List<string>();
                    for (int i = 0; i < BatchSize; i++) {
                        _logger?.Log(LogLevel.Information, $"Generating line #{i}");
                        string line = await GenerateLineAsync();
                        lines.Add(line);
                    }

                    _logger?.Log(LogLevel.Information, "Batch is generated.");

                    _logger?.Log(LogLevel.Information, "Writing batch lines to the output file...");

                    foreach (string line in lines) {
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
