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
            chunkLength = GetChunkLength(args.Length > 1 ? args[1] : null);
            
            MyFileReader fileReader = new MyFileReader(fileName, chunkLength);
            FileProcessor fileProcessor = new FileProcessor(fileReader);

            var startTime = DateTime.Now;

            bool finalStatus = fileProcessor.Process();

            var endTime = DateTime.Now;
            TimeSpan processingTime = endTime - startTime;

            Console.WriteLine($"Start:\t{startTime}\nEnd:\t{endTime}\nProcessingTime:\t{processingTime}");
            ConsoleInputOutput.ExitProgram(finalStatus ? ProgramFinishReason.Finished :ProgramFinishReason.Error);
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
            int chunkLength = 0;
            bool parsingResult = false;

            if (!string.IsNullOrWhiteSpace(userChunkLength))
                parsingResult = int.TryParse(userChunkLength, out chunkLength);

            while (chunkLength < 1 || !parsingResult)
            {
                Console.WriteLine("Please input length of chunks:");
                userChunkLength = Console.ReadLine();
                parsingResult = int.TryParse(userChunkLength, out chunkLength);

                if (chunkLength < 1)
                    Console.WriteLine("Chunk size must be a positive integer.");
            }
            return chunkLength;
        }

    }
}
