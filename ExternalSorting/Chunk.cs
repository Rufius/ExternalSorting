namespace ExternalSorting {
    internal class Chunk {
        public Chunk(int id) {
            Id = id;
            Lines = new List<string>();
        }

        public int Id { get; }
        public List<string> Lines { get; }
    }
}
