using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorting {
    public class ExternalSorter : IExternalSorter {
        private int _chunkSizeInBytes = 100;
        private int _parallelChunksNumber = 10; // number of chunks being sorted in parallel
        private readonly int NewlineBytesCount = Encoding.UTF8.GetByteCount(Environment.NewLine);

        private int _chunkCount = 0;

        private readonly ILogger _logger;

        public int ChunkSizeInBytes { get => _chunkSizeInBytes; set => _chunkSizeInBytes = value; }
        public int ParallelChunksNumber { get => _parallelChunksNumber; set => _parallelChunksNumber = value; }

        public ExternalSorter(ILogger logger) {
            _logger = logger;
        }

        public ExternalSorter(int chunkSizeInBytes, int parallelChunksNumber, ILogger logger) {
            _chunkSizeInBytes = chunkSizeInBytes;
            _parallelChunksNumber = parallelChunksNumber;
            _logger = logger;
        }

        public async Task SortAsync(string inputFilePath, string outputFilePath) {
            await SortChunksAsync(inputFilePath);
            await MergeChunksAsync(outputFilePath);
        }

        private async Task SortChunksAsync(string filePath) {
            _logger?.Log(LogLevel.Information, "Sorting chunks is starting");

            var chunks = new BlockingCollection<Chunk>(_parallelChunksNumber);

            // file reader task producing chunks
            var chunkProducerTask = Task.Run(async () => await ReadChunksAsync(filePath, chunks));

            // Sort chunks in parallel. We have ParallelChunksNumber number of sorter worker tasks.
            var sorterTasks = new Task[_parallelChunksNumber];
            for (int i = 0; i < _parallelChunksNumber; i++) {
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

                _logger?.Log(LogLevel.Information, $"Initializing chunk # {_chunkCount}");
                var chunk = new Chunk(_chunkCount); // initialize the first chunk

                while ((line = await streamReader.ReadLineAsync()) != null) {
                    _logger?.Log(LogLevel.Information, $"Reading line '{line}' into chunk # {chunk.Id}");

                    chunk.Lines.Add(line);
                    currentCnunkSize += Encoding.UTF8.GetByteCount(line) + NewlineBytesCount;

                    // if the chunk is full add it to the queue and initialize a new chunk
                    if (currentCnunkSize >= _chunkSizeInBytes) {
                        chunks.Add(chunk);

                        _logger?.Log(LogLevel.Information, $"Initializing chunk # {++_chunkCount}");
                        chunk = new Chunk(_chunkCount);
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
                _logger?.Log(LogLevel.Information, $"Reading lines into chunks is completed");
            }
        }

        private async Task MergeChunksAsync(string outputFilePath) {
            _logger?.Log(LogLevel.Information, "Merging chunks is starting");

            // create a priority queue
            var pq = new PriorityQueue<PriorityQueueElement, string>(_chunkCount, new CustomComparer());

            // output stream writer
            using (var outputStreamWriter = new StreamWriter(outputFilePath)) {
                // initialize stream readers for chunks
                var chunkStreamReaders = new List<StreamReader>();
                for (int i = 0; i <= _chunkCount; i++) {
                    _logger?.Log(LogLevel.Information, $"Reading chunk # {i} is starting");
                    chunkStreamReaders.Add(new StreamReader($"chunk_{i}.txt"));
                }

                // read first line of each chunk
                for (int i = 0; i <= _chunkCount; i++) {
                    var line = await chunkStreamReaders[i].ReadLineAsync();
                    _logger?.Log(LogLevel.Information, $"Reading line '{line}' from chunk # {i}");

                    if (line != null) {
                        pq.Enqueue(new PriorityQueueElement(line, i), line);
                    }
                }

                // take min element and write into output until the priority queue is empty
                while (pq.Count > 0) {
                    // write minimum element line to the output file
                    var outputElement = pq.Dequeue();
                    _logger?.Log(LogLevel.Information, $"Writing line '{outputElement.Line}' to output file");
                    outputStreamWriter.WriteLine(outputElement.Line);

                    // replace it with the next line from the same chunk
                    string? newLine = await chunkStreamReaders[outputElement.ChunkIndex].ReadLineAsync();
                    if (newLine != null) {
                        pq.Enqueue(new PriorityQueueElement(newLine, outputElement.ChunkIndex), newLine);
                    }
                }

                // dispose all readers
                chunkStreamReaders.ForEach(streamReader => streamReader.Dispose());

                _logger?.Log(LogLevel.Information, "Merging chunks is completed");
            }
        }
    }
}
