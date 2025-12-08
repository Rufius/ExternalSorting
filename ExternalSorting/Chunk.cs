using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
