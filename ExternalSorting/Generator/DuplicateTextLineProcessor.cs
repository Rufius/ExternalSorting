namespace ExternalSorting.Generator {
    public class DuplicateTextLineProcessor : ITextLineProcessor {
        private readonly int MaxNumberOfIterationsBeforeDuplicate = 0;
        private readonly int MaxNumberOfDuplicates = 0;

        private string _duplicateText = string.Empty;
        private int _numberOfIterationsBeforeInsertingDuplicate;
        private int _numberOfDuplicateValues = 0;
        private int _duplicateIterationCounter = 0;
        private int _duplicateValuesCounter = 0;
        private Random _random = new Random();

        private readonly ILineGenerator _lineGenerator;
        public DuplicateTextLineProcessor(int maxNumberOfDuplicates, int maxNumberOfIterationsBeforeDuplicate, ILineGenerator lineGenerator) {
            MaxNumberOfDuplicates = maxNumberOfDuplicates;
            MaxNumberOfIterationsBeforeDuplicate = maxNumberOfIterationsBeforeDuplicate;
            _numberOfIterationsBeforeInsertingDuplicate = _random.Next(1, MaxNumberOfIterationsBeforeDuplicate);
            _lineGenerator = lineGenerator;
        }

        public async Task<string> GetTextLineAsync() {
            var text = string.Empty;

            if (_duplicateIterationCounter >= _numberOfIterationsBeforeInsertingDuplicate) {
                text = GetDuplicateText();
            } else {
                // generate new text line
                text = await _lineGenerator.GenerateAsync();

                // setup insertion of a new duplicate value 
                SetUpDuplicate(text);
            }

            _duplicateIterationCounter++;

            return text;
        }

        private void SetUpDuplicate(string text) {
            // setup new duplicate value if necessary
            if (_duplicateValuesCounter < _numberOfDuplicateValues) { return; }

            _duplicateValuesCounter = 0;
            _numberOfDuplicateValues = _random.Next(1, MaxNumberOfDuplicates); // how many times to insert this text
            _duplicateText = text;
        }

        private string GetDuplicateText() {
            _duplicateValuesCounter++;
            _duplicateIterationCounter = 0;
            _numberOfIterationsBeforeInsertingDuplicate = _random.Next(1, MaxNumberOfIterationsBeforeDuplicate); // insert duplicate value after that many lines
            return _duplicateText;
        }
    }
}
