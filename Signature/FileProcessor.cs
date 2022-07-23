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
        Dictionary<int, string> hashes;
        int? finalChunkCount;
        bool finalStatus;

        public FileProcessor(MyFileReader fileReader)
        {
            this.fileReader = fileReader;
            chunkQueue = new Queue<Chunk>();
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
                    if (finalChunkCount.HasValue && finalChunkCount.Value <= chunkCounter)
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
            SignatureProvider.ProcessQueue(chunkQueue, hashes);
        }

        private void ReadFileIntoQueue()
        {
            try
            {
                finalChunkCount = fileReader.ReadFile(chunkQueue);
            }
            catch (Exception e)
            {
                finalStatus = false;
                if (e is OutOfMemoryException)
                    e.PrintException(ErrorReason.OutOfMemory); 
                else if (e is ArgumentException)
                    e.PrintException(ErrorReason.InvalidFileName);
                else if (e is IOException)
                    e.PrintException(ErrorReason.CouldNotOpenFile);
                else
                    e.PrintException();
            }
        }
    }
}
