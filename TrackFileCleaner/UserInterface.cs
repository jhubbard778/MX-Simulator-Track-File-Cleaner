using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrackFileCleaner
{
    internal class UserInterface
    {
        private const int MAX_CHARACTER_LIMIT = 80;

        public static void PromptEnter()
        {
            PrintHeader();
            PrintDescription();
            Console.WriteLine("> Press the enter key when you are ready to clean...\n");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyInfo = Console.ReadKey();
            }
        }

        private static void PrintHeader()
        {
            string outline = new string('#', MAX_CHARACTER_LIMIT);
            
            string CenterText = "MX Simulator Track File Cleaner";

            int spaces = outline.Length - CenterText.Length;
            int padLeft = spaces / 2 + CenterText.Length;

            CenterText = CenterText.PadLeft(padLeft);
            outline = Colors.cyan + outline + Colors.normal;

            Console.WriteLine(outline);
            Console.WriteLine(CenterText);
            Console.WriteLine(outline + '\n');
        }

        private static void PrintDescription()
        {
            string description = "- The purpose of this program is to clean up any unused files that a track may have inside its folder.\n\n";

            description += "- The program will go through every folder in the directory of the application. " +
                "Make sure the application is in the directory you need it to be.\n\n";
            description += "- The program will check one subfolder deep for other tracks, " +
                "anything subfolder after will be skipped.  Any files not being used by the track will be deleted.\n\n";

            description = WrapString(description, MAX_CHARACTER_LIMIT);

            description += Colors.cyan;
            description += new string('-', MAX_CHARACTER_LIMIT);
            description += Colors.normal + '\n';

            Console.WriteLine(description);
        }

        private static string WrapString(string str, int wrapLength)
        {
            char[] chars = str.ToCharArray();

            int offset = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                // Here we calculate the next word's length to determine if we should wrap before the next word
                int NextSpaceIndex = Array.IndexOf(chars, ' ', i);
                int NextWordLength = -1;
                if (NextSpaceIndex > -1)
                {
                    int SpaceAfterNextSpaceIndex = Array.IndexOf(chars, ' ', NextSpaceIndex + 1);
                    if (SpaceAfterNextSpaceIndex > -1)
                    {
                        NextWordLength = SpaceAfterNextSpaceIndex - NextSpaceIndex - 1;
                    }
                }

                char ch = chars[i];
                if (ch == '\n') { offset = 0; continue; }

                // If we hit the wrap condition
                if (NextWordLength > -1 && offset + NextWordLength >= wrapLength - 1)
                {
                    if (i + 1 == chars.Length) break;

                    char nextCharacter = chars[i + 1];

                    // If the next character is a newline or space reset the offset and increment i
                    if (nextCharacter == '\n' || nextCharacter == ' ')
                    {
                        // if it's a space add a newline in the designated spot
                        if (nextCharacter == ' ')
                        {
                            chars[i + 1] = '\n';
                        }

                        offset = 0;
                        i++;
                        continue;
                    }
                }

                offset++;
            }

            return new string(chars);
        }
    }
}
