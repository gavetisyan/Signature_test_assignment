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

        public FileProcessor(MyFileReader fileReader)
        {
            this.fileReader = fileReader;
            chunkQueue = new Queue<Chunk>();
            hashes = new Dictionary<int, string>();
        }

        public void Process()
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
            SignatureProvider.StopIfQueueIsEmpty();

            // finish computing
            foreach (Thread thread in computingThreads)
            {
                thread.Join();
            }

            // finish output
            outputingThread.Join();
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
            int numberInQueue = 0;
            bool continueReading = true;
            using (FileStream fileStream = fileReader.Initialize())
            {
                if (fileStream == null)
                {
                    ConsoleInputOutput.ExitProgram(ProgramFinishReason.Error);
                    return;
                }

                while (continueReading)
                {
                    bool enqueueingSuccessful = false;
                    int attempts = 0;
                    while (!enqueueingSuccessful)
                    {
                        try
                        {
                            (byte[] bytes, bool moreToRead) = fileReader.ReadNextChunk(fileStream);

                            var chunk = new Chunk(bytes, numberInQueue++);
                            lock (chunkQueue)
                            {
                                chunkQueue.Enqueue(chunk);
                                enqueueingSuccessful = true;
                            }
                            continueReading = moreToRead;
                        }
                        catch (OutOfMemoryException)
                        {
                            if (attempts > 50)
                                throw;
                            attempts++;
                        }
                        // if queue is too long, wait for computing threads to remove some elements
                        if (!enqueueingSuccessful)
                            Thread.Sleep(50);
                    }
                }
                finalChunkCount = numberInQueue;
            }
        }
    }
}
