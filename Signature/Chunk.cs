using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signature
{
    internal class Chunk
    {
        byte[] Bytes { get; }
        int queueNumber { get; }

        public Chunk(byte[] bytes, int queueNumber)
        {
            Bytes = bytes;
            this.queueNumber = queueNumber;
        }
    }
}
