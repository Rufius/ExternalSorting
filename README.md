# External Sorting

Use External Merge Sort to sort a text file. 
There are two stages:

### Stage #1 - Sorting chunks
- Split the file into chunks of N lines that fit into memory
- Read the first chunk from the file
- Sort it with the native method
- Write to the temporary file(e.g, number-of-chunk.txt)
- Repeat for every chunk

### Stage #2 - Merging chunks
- When all chunks are sorted, merge the chunks in the following way
- Create a min heap priority queue with a length equal to the number of chunks 
- Create a StreamReader for each chunk file
- Create a single StreamWriter for the output file
- Enter a loop doing the following
- Read the line from each chunk into the priority queue
- Pop the min element from the priority queue and write it into the output file
- Read the next line from the same chunk to which the line we popped belonged
- Repeat until the priority queue is empty
- Close all the stream readers and the writer

## Ways to improve performance
- Parallelize chunk sorting
- Leverage multithreading for the merging stage
- ???
