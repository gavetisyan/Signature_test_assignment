using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Signature
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            int chunkLength = 0;

            fileName = GetFileLocation(args.Length > 0 ? args[0] : null);
            while (chunkLength < 1)
            {
                chunkLength = GetChunkLength(args.Length > 1 ? args[1] : null);
                if (chunkLength < 1)
                    Console.WriteLine("Chunk size must be positive.");
            }
            MyFileReader fileReader = new MyFileReader(fileName, chunkLength);
            using (FileStream fileStream = fileReader.Initialize())
            {
                if (fileStream == null)
                {
                    Console.WriteLine("Error occured. Press any key to exit.");
                    Console.ReadKey();
                }
                else
                {
                    FileProcessor fileProcessor = new FileProcessor(fileReader, fileStream);
                    fileProcessor.Process();
                }
            }
        }

        private static string GetFileLocation(string fileName)
        {
            while (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("Please input location of the file:");
                fileName = Console.ReadLine();
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("File with this path does not exist.");
                    fileName = null;
                }
            }
            return fileName;
        }

        private static int GetChunkLength(string userChunkLength)
        {
            if (string.IsNullOrEmpty(userChunkLength))
            {
                Console.WriteLine("Please input length of chunks:");
                userChunkLength = Console.ReadLine();
            }
            int chunkLength = int.Parse(userChunkLength);
            return chunkLength;
        }

    }
}
