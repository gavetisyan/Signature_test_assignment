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

        public MyFileReader(string fileName, int chunkSize)
        {
            this.chunkSize = chunkSize;
            this.fileName = fileName;
        }

        public FileStream Initialize()
        {
            FileStream fileStream = File.Open(fileName, FileMode.Open);
            
            return fileStream;
        }

        internal int ReadFile(Queue<Chunk> chunkQueue)
        {
            int numberInQueue = 0;
            using (FileStream fileStream = Initialize())
            {
                bool continueReading = true;
                while (continueReading)
                {
                    bool readingSuccessful = false;
                    bool enqueueingSuccessful = false;
                    int attempts = 0;
                    byte[] bytes = null;
                    while (!enqueueingSuccessful)
                    {
                        try
                        {
                            if (!readingSuccessful)
                                (bytes, continueReading) = ReadNextChunk(fileStream);
                            readingSuccessful = true;
                            var chunk = new Chunk(bytes, numberInQueue);
                            lock (chunkQueue)
                            {
                                chunkQueue.Enqueue(chunk);
                                enqueueingSuccessful = true;
                            }
                            numberInQueue++;
                        }
                        catch (OutOfMemoryException)
                        {
                            // if queue is too long, wait for computing 
                            // threads to remove some elements
                            if (attempts > 5000)
                                throw;
                            attempts++;
                        }
                        if (!enqueueingSuccessful)
                            Thread.Sleep(50);
                    }
                }
                return numberInQueue;
            }
        }

        private (byte[], bool) ReadNextChunk(FileStream stream)
        {
            byte[] bytes = new byte[chunkSize];
            int readBytesCount = stream.Read(bytes, 0, chunkSize);
            bool moreFileToRead = readBytesCount == chunkSize;
            return (bytes, moreFileToRead);
        }
    }
}
