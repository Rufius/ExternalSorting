using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorting {
    public class ExternalSorter {
        private readonly int ChunkSizeInBytes = 0;
        private readonly int NewlineBytesCount = 0;

        const int ParallelChunksNumber = 10; // number of chunks being sorted in parallel

        int _chunkCount = 0;
        public ExternalSorter(int chunkSizeInBytes) {
            ChunkSizeInBytes = chunkSizeInBytes;
            NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);
        }

        public async Task SortAsync(string filePath) {
            SortChunks(filePath);
            await MergeChunksAsync();
        }
        private void SortChunks(string filePath) {
            var chunks = new BlockingCollection<Chunk>(ParallelChunksNumber);

            // file reader task producing chunks
            var chunksProducerTask = Task.Run(async () => {
                await ReadChunksAsync(filePath, chunks);
            });

            // Sort chunks in parallel. We have ParallelChunksNumber number of sorter worker tasks.
            var sorterTasks = new Task[ParallelChunksNumber];
            for (int i = 0; i < ParallelChunksNumber; i++) {
                sorterTasks[i] = Task.Run(() => {
                    foreach (var chunk in chunks.GetConsumingEnumerable()) {
                        chunk.Lines.Sort(new CustomComparer()); // sort the chunk
                        File.WriteAllLines($"chunk_{chunk.Id}.txt", chunk.Lines); // write into temporary file
                    }
                });
            }

            Task.WaitAll(sorterTasks);
            chunksProducerTask.Wait();
        }

        private async Task ReadChunksAsync(string filePath, BlockingCollection<Chunk> chunks) {
            using (var streamReader = new StreamReader(filePath)) {
                string? line = "";
                _chunkCount = 0;
                var chunk = new Chunk(_chunkCount);
                long currentCnunkSize = 0;

                do {
                    line = await streamReader.ReadLineAsync(); // read next line
                    if (line != null) { 
                        chunk.Lines.Add(line);
                        currentCnunkSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;
                    }

                    if (currentCnunkSize >= ChunkSizeInBytes) {
                        chunks.Add(chunk);
                        chunk = new Chunk(++_chunkCount);
                        currentCnunkSize = 0;
                    }
                } while (line != null);

                // last chunk
                if (chunk.Lines.Count > 0) {
                    chunks.Add(chunk);
                } else {
                    _chunkCount--; // the last initiated chunk was empty, so do not count it
                }

                chunks.CompleteAdding();
            }
        }

        private async Task MergeChunksAsync() {
            // create a priority queue
            var pq = new PriorityQueue<PriorityQueueElement, string>(_chunkCount, new CustomComparer());

            // output stream writer
            using (var outputStreamWriter = new StreamWriter("output.txt")) {
                // initialize stream readers for chunks
                var chunkStreamReaders = new List<StreamReader>();
                for (int i = 0; i <= _chunkCount; i++) {
                    chunkStreamReaders.Add(new StreamReader($"chunk_{i}.txt"));
                }

                // read first line of each chunk
                for (int i = 0; i <= _chunkCount; i++) {
                    var line = await chunkStreamReaders[i].ReadLineAsync();

                    if (line != null) {
                        pq.Enqueue(new PriorityQueueElement(line, i), line);
                    }
                }

                // Take min element and write into output until the priority queue is empty.
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
