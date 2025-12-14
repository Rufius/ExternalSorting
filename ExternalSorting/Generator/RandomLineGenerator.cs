using System;
using System.Text;

namespace ExternalSorting.Generator {
    public class RandomLineGenerator : ILineGenerator {
        private readonly Random _random = new Random();
        const string Letters = "abcdefghijklmnopqrstuvwxyz";

        public async Task<string> GenerateAsync() {
            int numberOfWords = _random.Next(1, 5);
            StringBuilder line = new StringBuilder();

            for (int i = 0; i < numberOfWords; i++) {
                int numberOfLetters = _random.Next(3, 10);
                StringBuilder word = new StringBuilder(numberOfLetters);

                for (int j = 0; j < numberOfLetters; j++) {
                    int letterIndex = _random.Next(Letters.Length - 1);
                    word.Append(Letters[letterIndex]);
                }

                line.Append(word.ToString());
                line.Append(" ");
            }

            //remove last space
            line.Remove(line.Length - 1, 1);

            return await Task.FromResult(line.ToString());
        }
    }
}
