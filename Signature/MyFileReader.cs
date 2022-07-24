using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Signature
{
    public class MyFileReader
    {
        private string fileName;
        private int chunkSize;
        private bool continueReading;

        private int timeOut = 50;

        public MyFileReader(string fileName, int chunkSize)
        {
            this.chunkSize = chunkSize;
            this.fileName = fileName;
            continueReading = true;
        }
        
        internal int ReadFile(Queue<Chunk> chunkQueue, Queue<Chunk> recycleQueue)
        {
            int numberInQueue = 0;
            using (FileStream fileStream = File.Open(fileName, FileMode.Open))
            {
                while (continueReading)
                {
                    bool readingSuccessful = false;
                    bool enqueueingSuccessful = false;
                    int attempts = 0;
                    byte[] bytes = null;
                    Chunk chunk = null;

                    while (!readingSuccessful)
                    {
                        if (chunk == null)
                        {
                            chunk = GetRecycledChunk(recycleQueue);
                        }
                        try
                        {
                            ReadBytes(fileStream, chunk, ref bytes);
                            readingSuccessful = true;
                        }
                        catch (OutOfMemoryException)
                        {
                            if (attempts > 5000 || numberInQueue == 0)
                                throw;
                            attempts++;
                        }
                        if (!readingSuccessful)
                            Thread.Sleep(timeOut);
                    }
                    while (!enqueueingSuccessful)
                    {
                        try
                        {
                            GetUpdatedOrNewChunk(ref chunk, bytes, numberInQueue);

                            lock (chunkQueue)
                            {
                                chunkQueue.Enqueue(chunk);
                                enqueueingSuccessful = true;
                            }
                            numberInQueue++;
                        }
                        catch (OutOfMemoryException)
                        {
                            if (attempts > 5000 || numberInQueue == 0)
                                throw;
                            attempts++;
                        }
                        // if queue is too long, wait for computing 
                        // threads to remove some elements
                        if (!enqueueingSuccessful)
                            Thread.Sleep(timeOut);
                    }
                }
                return numberInQueue;
            }
        }

        private void GetUpdatedOrNewChunk(ref Chunk chunk, byte[] bytes, int numberInQueue)
        {
            if (chunk == null)
            {
                chunk = new Chunk(bytes, numberInQueue);
            }
            else
            {
                chunk.Recycle(numberInQueue);
            }
        }

        private void ReadBytes(FileStream fileStream, Chunk chunk, ref byte[] bytes)
        {
            if (chunk == null)
            {
                bytes = new byte[chunkSize];
                ReadNextChunk(fileStream, bytes);
            }
            else
            {
                ReadNextChunk(fileStream, chunk.Bytes);
            }
        }

        private Chunk GetRecycledChunk(Queue<Chunk> recycleQueue)
        {
            Chunk chunk = null;
            if (recycleQueue.Count > 0)
            {
                lock (recycleQueue)
                {
                    if (recycleQueue.Count > 0)
                        chunk = recycleQueue.Dequeue();
                }
            }
            return chunk;
        }

        private void ReadNextChunk(FileStream stream, byte[] bytes)
        {
            int readBytesCount = stream.Read(bytes, 0, chunkSize);
            bool moreFileToRead = readBytesCount == chunkSize;
            continueReading = moreFileToRead;
        }
    }
}
