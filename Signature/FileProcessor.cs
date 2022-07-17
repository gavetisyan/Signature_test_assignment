using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Signature
{
    internal class FileProcessor
    {
        MyFileReader fileReader;
        FileStream fileStream;
        Queue<Chunk> chunkQueue;
        Dictionary<int, string> hashes;
        int? finalChunkCount;

        public FileProcessor(MyFileReader fileReader, FileStream fileStream)
        {
            this.fileReader = fileReader;
            this.fileStream = fileStream;
            chunkQueue = new Queue<Chunk>();
            hashes = new Dictionary<int, string>();
        }

        public void Process()
        {
            Thread readingThread = new Thread(ReadFileIntoQueue);
            readingThread.Start();

            Thread computingThread = new Thread(ProcessQueue);
            computingThread.Start();

            Thread outputingThread = new Thread(OutputResultsInOrder);
            outputingThread.Start();

            readingThread.Join();
            SignatureProvider.StopIfQueueIsEmpty();
            computingThread.Join();
            outputingThread.Join();
        }

        private void OutputResultsInOrder()
        {
            int i = 0;

            while (hashes.Count > 0 || !finalChunkCount.HasValue || finalChunkCount.Value > i)
            {
                bool requiredEntryExists = hashes.TryGetValue(i, out string hash);
                
                while (!requiredEntryExists)
                {
                    requiredEntryExists = hashes.TryGetValue(i, out hash);
                    Thread.Sleep(5);
                }

                lock (hashes)
                {
                    hashes.Remove(i);
                }
                Console.WriteLine($"{i}:\t{hash}");
                i++;
            }
        }

        private void ProcessQueue()
        {
            SignatureProvider.ProcessQueue(chunkQueue, hashes);
        }

        private void ReadFileIntoQueue()
        {
            int numberInQueue = 0;
            bool continueReading = true;
            while (continueReading)
            {
                while (chunkQueue.Count > 10) // proper limit
                    Thread.Sleep(10);
                (byte[] bytes, bool moreToRead) = fileReader.ReadNextChunk(fileStream);

                lock (chunkQueue)
                {
                    chunkQueue.Enqueue(new Chunk(bytes, numberInQueue++));
                }
            // todo: limit size of queue so that not ALL of data is stored in RAM
                continueReading = moreToRead;
            }
            finalChunkCount = numberInQueue;
        }
    }
}
