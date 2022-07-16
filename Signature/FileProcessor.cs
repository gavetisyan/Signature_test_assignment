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

        public FileProcessor(MyFileReader fileReader, FileStream fileStream)
        {
            this.fileReader = fileReader;
            this.fileStream = fileStream;
            chunkQueue = new Queue<Chunk>();
        }

        public void Process()
        {
            Thread readingThread = new Thread(ReadFileIntoQueue);
            readingThread.Start();

            readingThread.Join();
        }

        private void ReadFileIntoQueue()
        {
            int numberInQueue = 0;
            bool continueReading = true;
            while (continueReading)
            {
                continueReading = AddChunkToQueue(numberInQueue++);
            }
        }

        private bool AddChunkToQueue(int numberInQueue)
        {
            (byte[] bytes, bool moreToRead) = fileReader.ReadNextChunk(fileStream);
            chunkQueue.Enqueue(new Chunk(bytes, numberInQueue));
            return moreToRead;
        }
    }
}
