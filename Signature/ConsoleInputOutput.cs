using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signature
{
    public enum Reason
    {
        Error,
        Finished
    }

    class ConsoleInputOutput
    {
        private static Dictionary<Reason, string> exitReasonStrings = new Dictionary<Reason, string>{
            {Reason.Error, "Error occured. Press any key to exit."},
                { Reason.Finished, "Program finished. Press any key to exit."} };
 

        public static void ExitProgram(Reason reason)
        {
            Console.WriteLine(exitReasonStrings[reason]);
            Console.ReadKey();
        }
    }
}
