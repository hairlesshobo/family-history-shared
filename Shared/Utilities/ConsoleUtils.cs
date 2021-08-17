using System;

namespace Archiver.Shared.Utilities
{
    public static class ConsoleUtils
    {
        public static bool YesNoConfirm(string message, bool yesDefault, bool clearScreen)
        {
            if (clearScreen)
                Console.Clear();

            if (!message.EndsWith("?"))
                message += "?";

            Console.Write($"{message} (");
            
            if (yesDefault)
            {
                Formatting.WriteC(ConsoleColor.Blue, "YES");
                Console.Write("/no");
            }
            else
            {
                Console.Write("yes/");
                Formatting.WriteC(ConsoleColor.Blue, "NO");
            }

            Console.Write(") ");

            bool prevCtrlValue = Console.TreatControlCAsInput;

            Console.TreatControlCAsInput = false;
            Console.CursorVisible = true;
            string response = Console.ReadLine();
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = prevCtrlValue;

            bool responseWasYes = response.ToLower().StartsWith("yes");

            if (!responseWasYes && yesDefault && response.Trim().Length == 0)
                responseWasYes = true;

            if (clearScreen)
                Console.Clear();

            return responseWasYes;
        }
    }
}