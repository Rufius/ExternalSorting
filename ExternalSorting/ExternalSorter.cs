using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting {
    public class ExternalSorter {
        int _chunkSize = 0;

        public ExternalSorter(int chunkSize) {
            _chunkSize = chunkSize;
        }

        public async Task SortAsync(string filePath) {
            await SortChunksAsync(filePath);
            MergeChunks();
        }

        private async Task SortChunksAsync(string filePath) {
            using (var streamReader = new StreamReader(filePath)) {
                var chunkLines = new List<string>(); //chunk buffer
                string? line = "";
                int chunkCount = 0;

                do {
                    line = await streamReader.ReadLineAsync(); // read next line
                    if (line != null) { chunkLines.Add(line); }

                    if (chunkLines.Count >= _chunkSize) {
                        chunkLines.Sort(new CustomComparer()); // sort the chunk
                        File.WriteAllLines($"chunk_{chunkCount}.txt", chunkLines); // write into temporary file
                        chunkCount++;
                        chunkLines.Clear(); // clear the buffer
                    }

                } while (line != null);
            }
        }

        private void MergeChunks() {
            throw new NotImplementedException();
        }
    }
}
