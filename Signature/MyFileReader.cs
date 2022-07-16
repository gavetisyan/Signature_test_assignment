using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(fileName, FileMode.Open);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Please provide a valid file name.");
                Console.WriteLine(e.Message + e.StackTrace);
            }
            catch (IOException e)
            {
                Console.WriteLine("Could not open file.");
                Console.WriteLine(e.Message + e.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }
            return fileStream;
        }

        public (byte[], bool) ReadNextChunk(FileStream stream)
        {
            byte[] bytes = new byte[chunkSize];
            int readBytesCount = stream.Read(bytes, 0, chunkSize);
            bool moreFileToRead = readBytesCount == chunkSize;
            return (bytes, moreFileToRead);
        }
    }
}
