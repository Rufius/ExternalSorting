using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorting.Generator {
    public class FileGenerator : IFileGenerator {
        private readonly int NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);
        private readonly int MaxNumber;

        const int NumberOfLinesPerBatch = 10;

        private readonly ITextLineProcessor _textLineProcessor;
        private readonly ILogger _logger;

        private readonly Random _random = new Random();

        private BlockingCollection<string> _lines = new BlockingCollection<string>();

        public FileGenerator(int maxNumber, ITextLineProcessor textLineProcessor, ILogger logger) {
            MaxNumber = maxNumber;
            _textLineProcessor = textLineProcessor;
            _logger = logger;
        }

        public async Task GenerateAsync(string filePath, int fileSize) {
            var producerTask = Task.Run(async () => await ProduceLinesAsync(fileSize));
            var writerTask = Task.Run(async () => await WriteLinesAsync(filePath));

            await writerTask;
            await producerTask;
        }

        private async Task ProduceLinesAsync(int fileSize) {
            _logger?.Log(LogLevel.Information, "Starting generating lines");

            int currentSize = 0;
            string text = string.Empty;
            while (currentSize < fileSize) {
                text = await _textLineProcessor.GetTextLineAsync();
                int number = _random.Next(MaxNumber);
                string line = $"{number}. {text}";

                _lines.Add(line);

                _logger?.Log(LogLevel.Information, $"Line generated: '{line}'");

                currentSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;
            }

            // make sure that file contains at least one duplicate value (in case of small files, less than 100 lines)
            _lines.Add($"{_random.Next(MaxNumber)}. {text}");

            _lines.CompleteAdding();
            _logger?.Log(LogLevel.Information, "Lines generation is completed");
        }

        private async Task WriteLinesAsync(string filePath) {
            _logger?.Log(LogLevel.Information, "Starting writing lines");

            using (var streamWriter = new StreamWriter(filePath)) {
                foreach (var line in _lines.GetConsumingEnumerable()) {
                    await streamWriter.WriteLineAsync(line);
                    _logger?.Log(LogLevel.Information, $"Line written: '{line}'");
                }
            }

            _logger?.Log(LogLevel.Information, "Writing lines is completed");
        }
    }
}
