using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signature
{
    public enum ProgramFinishReason
    {
        Error,
        Finished
    }

    public enum ErrorReason
    {
        InvalidFileName,
        CouldNotOpenFile,
        OutOfMemory
    }

    static class ConsoleInputOutput
    {
        private static Dictionary<ProgramFinishReason, string> exitReasonStrings =
            new Dictionary<ProgramFinishReason, string>{
            {ProgramFinishReason.Error, "Error occured. Press any key to exit."},
            {ProgramFinishReason.Finished, "Program finished. Press any key to exit."}
        };

        private static Dictionary<ErrorReason, string> errorTexts =
            new Dictionary<ErrorReason, string> {
                {ErrorReason.InvalidFileName, "Invalid file name." },
                {ErrorReason.CouldNotOpenFile, "Could not open file."},
                { ErrorReason.OutOfMemory, "Out of memory. Perhaps chunk size is too long." }
            };

        public static void ExitProgram(ProgramFinishReason reason)
        {
            Console.WriteLine(exitReasonStrings[reason]);
            Console.ReadKey();
        }

        internal static void PrintException(this Exception e, ErrorReason? reason = null)
        {
            if (e is OutOfMemoryException)
                reason = ErrorReason.OutOfMemory;
            else if (e is ArgumentException)
                reason = ErrorReason.InvalidFileName;
            else if (e is IOException)
                reason = ErrorReason.InvalidFileName;

            if (reason.HasValue)
                Console.WriteLine(errorTexts[reason.Value]);
            Console.WriteLine(e.Message + "\n" + e.StackTrace);
        }
    }
}
