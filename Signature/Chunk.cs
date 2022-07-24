using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Signature
{
    internal class Chunk
    {
        public byte[] Bytes { get; }
        public int NumberInQueue { get; private set; }

        public Chunk(byte[] bytes, int queueNumber)
        {
            Bytes = bytes;
            this.NumberInQueue = queueNumber;
        }

        public void Recycle(int newQueueNumber)
        {
            NumberInQueue = newQueueNumber;
        }

        public string GetChunkHash()
        {
            byte[] hash;
            using (SHA256 hashingFunction = SHA256.Create())
            {
                hash = hashingFunction.ComputeHash(Bytes);
            }
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
