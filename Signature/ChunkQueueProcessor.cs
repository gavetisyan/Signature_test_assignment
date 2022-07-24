using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Signature
{
    class ChunkQueueProcessor
    {
        private static bool stopProcessing = false;

        public static void StopIfQueueIsEmpty()
        {
            stopProcessing = true;
        }

        public static void ProcessQueue(Queue<Chunk> chunkQueue,
            Queue<Chunk> recycleQueue, Dictionary<int, string> hashes)
        {
            while (!stopProcessing)
            {
                while (chunkQueue.Count > 0)
                {
                    ProcessQueueElement(chunkQueue, recycleQueue, hashes);
                }
            }
        }

        private static void ProcessQueueElement(Queue<Chunk> chunkQueue, Queue<Chunk> recycleQueue, Dictionary<int, string> hashes)
        {
            Chunk chunk;
            lock (chunkQueue)
            {
                if (chunkQueue.Count > 0)
                    chunk = chunkQueue.Dequeue();
                else
                    return;
            }
            string hash = chunk.GetChunkHash();
            lock (hashes)
            {
                hashes.Add(chunk.NumberInQueue, hash);
            }
            lock (recycleQueue)
            {
                recycleQueue.Enqueue(chunk);
            }
        }
    }
}
