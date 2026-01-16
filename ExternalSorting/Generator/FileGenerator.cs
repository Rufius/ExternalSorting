using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorting.Generator {
    public class FileGenerator {
        private readonly int NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);
        private readonly int MaxNumber;

        const int NumberOfLinesPerBatch = 10;

        private readonly ILineGenerator _lineGenerator;
        private readonly ILogger _logger;

        private readonly Random _random;
        private readonly DuplicatesProcessor _duplicatesProcessor;

        private BlockingCollection<string> _lines = new BlockingCollection<string>();

        public FileGenerator(int maxNumber, ILineGenerator lineGenerator, ILogger logger) {
            MaxNumber = maxNumber;
            _lineGenerator = lineGenerator;
            _logger = logger;

            _random = new Random();
            _duplicatesProcessor = new DuplicatesProcessor(_random, 4, 100);
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

            var duplicatesProcessor = new DuplicatesProcessor(_random, 4, 100);

            while (currentSize < fileSize) {
                string text = await GetTextLineAsync(duplicatesProcessor); 
                int number = _random.Next(MaxNumber);
                string line = $"{number}. {text}";

                _lines.Add(line);

                _logger?.Log(LogLevel.Information, $"Line generated: '{line}'");

                currentSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;

                duplicatesProcessor.TickIteration();
            }

            // make sure that file contains at least one duplicate value (in case of small files, less than 100 lines)
            _lines.Add($"{_random.Next(MaxNumber)}. {duplicatesProcessor.GetDuplicateText()}");

            _lines.CompleteAdding();
            _logger?.Log(LogLevel.Information, "Lines generation is complete");
        }

        private async Task<string> GetTextLineAsync(DuplicatesProcessor duplicatesProcessor) {
            var text = string.Empty;

            if (duplicatesProcessor.ShouldInsertDuplicate) {
                text = duplicatesProcessor.GetDuplicateText();
            } else {
                // generate new text line
                text = await _lineGenerator.GenerateAsync();

                // setup insertion of a new duplicate value 
                duplicatesProcessor.SetUpDuplicate(text);
            }

            return text;
        }

        private async Task WriteLinesAsync(string filePath) {
            _logger?.Log(LogLevel.Information, "Starting writing lines");

            using (var streamWriter = new StreamWriter(filePath)) {
                foreach (var line in _lines.GetConsumingEnumerable()) {
                    await streamWriter.WriteLineAsync(line);
                    _logger?.Log(LogLevel.Information, $"Line written: '{line}'");
                }
            }

            _logger?.Log(LogLevel.Information, "Writing lines is complete");
        }

        private class DuplicatesProcessor {
            private readonly int MaxNumberOfIterationsBeforeDuplicate = 0;
            private readonly int MaxNumberOfDuplicates = 0;

            private string _duplicateText = string.Empty;
            private int _numberOfIterationsBeforeInsertingDuplicate;
            private int _numberOfDuplicateValues = 0;
            private int _duplicateIterationCounter = 0;
            private int _duplicateValuesCounter = 0;
            private Random _random;
            public DuplicatesProcessor(Random random, int maxNumberOfDuplicates, int maxNumberOfIterationsBeforeDuplicate) {
                _random = random;
                MaxNumberOfDuplicates = maxNumberOfDuplicates;
                MaxNumberOfIterationsBeforeDuplicate = maxNumberOfIterationsBeforeDuplicate;
                _numberOfIterationsBeforeInsertingDuplicate = _random.Next(1, MaxNumberOfIterationsBeforeDuplicate);
            }

            public bool ShouldInsertDuplicate => _duplicateIterationCounter >= _numberOfIterationsBeforeInsertingDuplicate;

            public void SetUpDuplicate(string text) {
                // setup new duplicate value if necessary
                if (_duplicateValuesCounter < _numberOfDuplicateValues) { return; }

                _duplicateValuesCounter = 0;
                _numberOfDuplicateValues = _random.Next(1, MaxNumberOfDuplicates); // how many times to insert this text
                _duplicateText = text;
            }

            public string GetDuplicateText() {
                _duplicateValuesCounter++;
                _duplicateIterationCounter = 0;
                _numberOfIterationsBeforeInsertingDuplicate = _random.Next(1, MaxNumberOfIterationsBeforeDuplicate); // insert duplicate value after that many lines
                return _duplicateText;
            }

            public void TickIteration() {
                _duplicateIterationCounter++;
            }
        }
    }
}
