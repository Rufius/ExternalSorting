using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting {
    internal class PriorityQueueElement {
        public PriorityQueueElement(string line, int chunkIndex) {
            Line = line;
            ChunkIndex = chunkIndex;
        }

        public string Line { get; }

        public int ChunkIndex { get; }
    }
}
