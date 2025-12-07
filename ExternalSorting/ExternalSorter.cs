using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting {
    public class ExternalSorter {
        private readonly int ChunkSize = 0;
        int _chunkCount = 0;
        public ExternalSorter(int chunkSize) {
            ChunkSize = chunkSize;
        }

        public async Task SortAsync(string filePath) {
            await SortChunksAsync(filePath);
            await MergeChunks();
        }

        private async Task SortChunksAsync(string filePath) {
            using (var streamReader = new StreamReader(filePath)) {
                var chunkLines = new List<string>(); //chunk buffer
                string? line = "";
                _chunkCount = 0;

                do {
                    line = await streamReader.ReadLineAsync(); // read next line
                    if (line != null) { chunkLines.Add(line); }

                    if (chunkLines.Count >= ChunkSize) {
                        chunkLines.Sort(new CustomComparer()); // sort the chunk
                        File.WriteAllLines($"chunk_{_chunkCount}.txt", chunkLines); // write into temporary file
                        _chunkCount++;
                        chunkLines.Clear(); // clear the buffer
                    }

                } while (line != null);
            }
        }

        private async Task MergeChunks() {
            // create a priority queue
            var pq = new PriorityQueue<PriorityQueueElement, string>(_chunkCount, new CustomComparer());

            // output stream writer
            using (var outputStreamWriter = new StreamWriter("output.txt")) {
                // initialize stream readers for chunks
                var chunkStreamReaders = new List<StreamReader>();
                for (int i = 0; i < _chunkCount; i++) {
                    chunkStreamReaders.Add(new StreamReader($"chunk_{i}.txt"));
                }

                // read first line of each chunk
                for (int i = 0; i < _chunkCount; i++) {
                    var line = await chunkStreamReaders[i].ReadLineAsync();
                    pq.Enqueue(new PriorityQueueElement(line, i), line);
                }

                // take min element and write into output until PQ is empty
                while (pq.Count > 0) {
                    // write minimum element line to the output file
                    var outputElement = pq.Dequeue();
                    outputStreamWriter.WriteLine(outputElement.Line);

                    // replace it with the next line from the same chunk
                    string? newLine = await chunkStreamReaders[outputElement.ChunkIndex].ReadLineAsync();
                    if (newLine != null) {
                        pq.Enqueue(new PriorityQueueElement(newLine, outputElement.ChunkIndex), newLine);
                    }
                }

                // dispose all readers
                chunkStreamReaders.ForEach(streamReader => streamReader.Dispose());
            }
        }
    }
}
