using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorting.Generator {
    public class FileGenerator {
        private readonly int NewlineBytesCount;
        private readonly int MaxNumber;
        const int NumberOfLinesPerBatch = 10;
        Random _random = new Random();

        private readonly ILineGenerator _lineGenerator;
        private readonly ILogger _logger;

        private BlockingCollection<string> _lines = new BlockingCollection<string>();

        public FileGenerator(int maxNumber, ILineGenerator lineGenerator, ILogger logger) {
            MaxNumber = maxNumber;
            NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);
            _lineGenerator = lineGenerator;
            _logger = logger;
        }

        public async Task Generate(string filePath, int fileSize) {
            Task producerTask = Task.Run(async () => await ProduceLinesAsync(fileSize));
            Task writerTask = Task.Run(async () => await WriteLinesAsync(filePath));

            await writerTask;
            await producerTask;
        }

        private async Task ProduceLinesAsync(int fileSize) {
            _logger.Log(LogLevel.Information, "Starting generating lines");

            int currentSize = 0;

            var duplicatesConfig = new DuplicatesConfig(_random, 4, 100);

            while (currentSize < fileSize) {
                string text = await GetTextLineAsync(duplicatesConfig); 
                int number = _random.Next(MaxNumber);
                string line = $"{number}. {text}";

                _lines.Add(line);

                _logger.Log(LogLevel.Information, $"Line generated: '{line}'");

                currentSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;

                duplicatesConfig.TickIteration();
            }

            // make sure that file contains at least one duplicate value
            _lines.Add($"{_random.Next(MaxNumber)}. {duplicatesConfig.GetDuplicateText()}");

            _lines.CompleteAdding();
            _logger.Log(LogLevel.Information, "Lines generation is complete");
        }

        private async Task<string> GetTextLineAsync(DuplicatesConfig config) {
            var text = string.Empty;

            if (config.ShouldInsertDuplicate) {
                text = config.GetDuplicateText();
            } else {
                // generate new text line
                text = await _lineGenerator.GenerateAsync();

                // if necessary setup insertion of duplicate values 
                config.SetUpIfNecessary(text);
            }

            return text;
        }

        private async Task WriteLinesAsync(string filePath) {
            _logger.Log(LogLevel.Information, "Starting writing lines");

            using (var streamWriter = new StreamWriter(filePath)) {
                foreach (var line in _lines.GetConsumingEnumerable()) {
                    await streamWriter.WriteLineAsync(line);
                    _logger.Log(LogLevel.Information, $"Line written: '{line}'");
                }
            }

            _logger.Log(LogLevel.Information, "Writing lines is complete");
        }

        private class DuplicatesConfig {
            private Random _random;
            private readonly int _maxNumberOfIterationsBeforeDuplicate = 0;
            private readonly int _maxNumberOfDuplicates = 0;
            public DuplicatesConfig(Random random, int maxNumberOfDuplicates, int maxNumberOfIterationsBeforeDuplicate) {
                _random = random;
                _maxNumberOfDuplicates = maxNumberOfDuplicates;
                _maxNumberOfIterationsBeforeDuplicate = maxNumberOfIterationsBeforeDuplicate;
            }

            public string _duplicateText = string.Empty;
            public int _numberOfIterationsBeforeInsertingDuplicate = 0;
            public int _numberOfDuplicateValues = 0;
            public int _duplicateIterationCounter = 0;
            public int _duplicateValuesCounter = 0;
            public bool _isInsertingDuplicates = false;

            public bool ShouldInsertDuplicate => _isInsertingDuplicates && (_duplicateIterationCounter == _numberOfIterationsBeforeInsertingDuplicate);

            public void SetUpIfNecessary(string text) {
                if (_isInsertingDuplicates) return;

                _duplicateText = text;
                _numberOfDuplicateValues = _maxNumberOfDuplicates; // how many times to insert this text
                _numberOfIterationsBeforeInsertingDuplicate = _maxNumberOfIterationsBeforeDuplicate; // insert duplicate value after that many lines
                _isInsertingDuplicates = true;
            }

            public string GetDuplicateText() {
                _duplicateValuesCounter--;

                // reset if necessary
                if (_duplicateValuesCounter == 0) {
                    _numberOfIterationsBeforeInsertingDuplicate = 0;
                    _numberOfDuplicateValues = 0;
                    _duplicateIterationCounter = 0;
                    _isInsertingDuplicates = false;
                }

                return _duplicateText;
            }

            public void TickIteration() {
                _numberOfIterationsBeforeInsertingDuplicate--;
            }
        }
    }
}
