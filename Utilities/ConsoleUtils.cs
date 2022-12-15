/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;

namespace FoxHollow.FHM.Shared.Utilities
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