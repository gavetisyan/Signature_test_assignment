using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Signature
{
    class SignatureProvider
    {
        private static bool stopProcessing = false;

        public static void StopIfQueueIsEmpty()
        {
            stopProcessing = true;
        }

        public static void ProcessQueue(Queue<Chunk> chunkQueue, Dictionary<int, string> hashes)
        {
            while (!stopProcessing)
            {
                while (chunkQueue.Count > 0)
                {
                    ProcessQueueElement(chunkQueue, hashes);
                }
            }
        }

        private static void ProcessQueueElement(Queue<Chunk> chunkQueue, Dictionary<int, string> hashes)
        {
            Chunk chunk;
            lock (chunkQueue)
            {
                if (chunkQueue.Count > 0)
                    chunk = chunkQueue.Dequeue();
                else
                    return;
            }
            byte[] hash = chunk.GetChunkHash();
            string stringHash = GetStringHash(hash);
            lock (hashes)
            {
                hashes.Add(chunk.NumberInQueue, stringHash);
            }
        }

        private static string GetStringHash(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
