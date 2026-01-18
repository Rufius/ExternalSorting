using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorting {
    public class ExternalSorter : IExternalSorter {
        private readonly int ChunkSizeInBytes = 100;
        private readonly int ParallelChunksNumber = 10; // number of chunks being sorted in parallel
        private readonly int NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);

        private int _chunkCount = 0;

        public ExternalSorter() {}

        public ExternalSorter(int chunkSizeInBytes, int parallelChunksNumber) {
            ChunkSizeInBytes = chunkSizeInBytes;
            ParallelChunksNumber = parallelChunksNumber;
        }

        public async Task SortAsync(string filePath) {
            await SortChunksAsync(filePath);
            await MergeChunksAsync();
        }

        private async Task SortChunksAsync(string filePath) {
            var chunks = new BlockingCollection<Chunk>(ParallelChunksNumber);

            // file reader task producing chunks
            var chunkProducerTask = Task.Run(async () => await ReadChunksAsync(filePath, chunks));

            // Sort chunks in parallel. We have ParallelChunksNumber number of sorter worker tasks.
            var sorterTasks = new Task[ParallelChunksNumber];
            for (int i = 0; i < ParallelChunksNumber; i++) {
                sorterTasks[i] = Task.Run(async () => {
                    foreach (var chunk in chunks.GetConsumingEnumerable()) {
                        chunk.Lines.Sort(new CustomComparer()); // sort the chunk
                        await File.WriteAllLinesAsync($"chunk_{chunk.Id}.txt", chunk.Lines); // write into temporary file
                    }
                });
            }

            await Task.WhenAll(sorterTasks);
            await chunkProducerTask;
        }

        private async Task ReadChunksAsync(string filePath, BlockingCollection<Chunk> chunks) {
            using (var streamReader = new StreamReader(filePath)) {
                string? line = "";
                _chunkCount = 0;
                long currentCnunkSize = 0;
                var chunk = new Chunk(_chunkCount); // initialize the first chunk
                
                while ((line = await streamReader.ReadLineAsync()) != null) {
                    chunk.Lines.Add(line);
                    currentCnunkSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;

                    // if the chunk is full add it to the queue and initialize a new chunk
                    if (currentCnunkSize >= ChunkSizeInBytes) {
                        chunks.Add(chunk);
                        chunk = new Chunk(++_chunkCount);
                        currentCnunkSize = 0;
                    }
                }

                // last chunk
                if (chunk.Lines.Count > 0) {
                    chunks.Add(chunk);
                } else {
                    _chunkCount--; // the last initialized chunk was empty, so do not count it
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

                // take min element and write into output until the priority queue is empty
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
