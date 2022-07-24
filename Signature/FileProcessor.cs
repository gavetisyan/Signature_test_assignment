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
        Queue<Chunk> chunkQueue;
        Queue<Chunk> recycleQueue;
        Dictionary<int, string> hashes;
        int? finalChunkCount;
        bool finalStatus;

        public FileProcessor(MyFileReader fileReader)
        {
            this.fileReader = fileReader;
            chunkQueue = new Queue<Chunk>();
            recycleQueue = new Queue<Chunk>();
            hashes = new Dictionary<int, string>();
            finalStatus = true;
        }

        public bool Process()
        {
            // read file
            Thread readingThread = new Thread(ReadFileIntoQueue);
            readingThread.Start();


            List<Thread> computingThreads = new List<Thread>();
            int numberOfComputingThreads =
                (Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1);

            // compute hashes
            for (int i = 0; i < numberOfComputingThreads; i++)
            {
                var thread = new Thread(ProcessQueue);
                computingThreads.Add(thread);
                thread.Start();
            }

            // output result
            Thread outputingThread = new Thread(OutputResultsInOrder);
            outputingThread.Start();

            // finish reading file
            readingThread.Join();
            QueueFinishedLoading();

            // finish computing
            foreach (Thread thread in computingThreads)
            {
                thread.Join();
            }

            // finish output
            outputingThread.Join();
            return finalStatus;
        }

        private void QueueFinishedLoading()
        {
            SignatureProvider.StopIfQueueIsEmpty();
            if (!finalChunkCount.HasValue)
                finalChunkCount = 0;
        }

        private void OutputResultsInOrder()
        {
            int chunkCounter = 0;

            while (hashes.Count > 0 || !finalChunkCount.HasValue || finalChunkCount.Value > chunkCounter)
            {
                bool requiredEntryExists = hashes.TryGetValue(chunkCounter, out string hash);

                while (!requiredEntryExists)
                {
                    if (finalChunkCount.HasValue && finalChunkCount.Value <= chunkCounter || !finalStatus)
                        return;

                    Thread.Sleep(1);
                    requiredEntryExists = hashes.TryGetValue(chunkCounter, out hash);
                }

                lock (hashes)
                {
                    hashes.Remove(chunkCounter);
                }
                Console.WriteLine($"{chunkCounter}:\t{hash}");
                chunkCounter++;
            }
        }

        private void ProcessQueue()
        {
            try
            {
                SignatureProvider.ProcessQueue(chunkQueue, recycleQueue, hashes);
            }
            catch (Exception e)
            {
                finalStatus = false;
                e.PrintException();
            }
        }

        private void ReadFileIntoQueue()
        {
            try
            {
                finalChunkCount = fileReader.ReadFile(chunkQueue, recycleQueue);
            }
            catch (Exception e)
            {
                finalStatus = false;
                e.PrintException();
            }
        }
    }
}
