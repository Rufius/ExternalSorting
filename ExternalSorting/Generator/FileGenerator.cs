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

        Random _random = new Random();
       
        public FileGenerator(int maxNumber) {
            MaxNumber = maxNumber;
            NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);
        }

        public async Task Generate(string filePath, int fileSize) {
            int currentSize = 0;

            using (var streamWriter = new StreamWriter(filePath)) {
                while (currentSize < fileSize) {
                    string line = GenerateLine();
                    await streamWriter.WriteLineAsync(line);
                    currentSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;
                }
            }
        }

        private string GenerateLine() {
            return $"{_random.Next(MaxNumber)}. test test test";
        }
    }
}
