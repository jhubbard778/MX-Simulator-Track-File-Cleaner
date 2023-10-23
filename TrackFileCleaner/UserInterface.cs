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
            PrintOutlinePrompt('#', "MX Simulator Track File Cleaner", Colors.cyan);
            PrintDescription();
            Console.Write("> Press the enter key when you are ready to clean...");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyInfo = Console.ReadKey();
            }
            Console.WriteLine();
        }

        public static void PromptBackupDeletion(long[] items)
        {
            long ItemsDeleted = items[0];
            long BytesDeleted = items[1];

            string MoveString = (BytesDeleted > 0) ? $"to {Globals.BACKUP_FOLDER[(Environment.CurrentDirectory.Length + 1)..]}" : "";
            string BytesToString = Program.BytesToString(BytesDeleted);

            if (ItemsDeleted == 0) {
                Console.WriteLine(Colors.green + "- You're all clean :)" + Colors.normal);
            }

            PrintOutlinePrompt('#', $"Moved {items[0]} Items ({BytesToString}) {MoveString}", Colors.cyan);
            
            if (BytesDeleted > 0)
            {

                Console.Write("\n> Would you like to delete the backup folder? (y/n) | ");
                string UserInput = Console.ReadLine() ?? "";
                while (UserInput != "y" && UserInput != "n")
                {
                    Console.Write("\n> Would you like to delete the backup folder? (y/n) | ");
                    UserInput = Console.ReadLine() ?? "";
                }

                if (UserInput == "y")
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(Globals.BACKUP_FOLDER);
                    long totalSize = Program.DirSize(DirInfo);
                    string DirectorySize = Program.BytesToString(totalSize);

                    Program.DeleteBackupDirectory();
                    Console.WriteLine(Colors.red + $"\n - Deleted backup folder 'FILE-CLEANER-BACKUP' ({DirectorySize})" + Colors.normal);
                }
            }

            Console.Write("\n> Press any key to exit the program...");
            Console.ReadKey(true);
        }

        public static void PrintOutlinePrompt(char outlineChar, string text, string outlineColor = Colors.normal, string textColor = Colors.normal)
        {
            string outline = new string(outlineChar, MAX_CHARACTER_LIMIT);
            string CenterText = CenterStringFromOutline(outline, text);

            outline = outlineColor + outline + Colors.normal;
            CenterText = textColor + CenterText + Colors.normal;

            Console.WriteLine('\n' + outline);
            Console.WriteLine(CenterText);
            Console.WriteLine(outline + '\n');
        }

        private static string CenterStringFromOutline(string outline, string text)
        {
            int spaces = outline.Length - text.Length;
            int padLeft = spaces / 2 + text.Length;

            text = text.PadLeft(padLeft);
            return text;
        }

        private static void PrintDescription()
        {
            string description = "- The purpose of this program is to clean up any unused files that a track may have inside its folder.\n\n";

            description += "- The program will go through every folder in the directory of the application. " +
                "Make sure the application is in the directory you need it to be.\n\n";
            description += "- The program will check one subfolder deep for other tracks, " +
                "anything subfolder after will be skipped.  Any files not being used by the track will be deleted.\n\n";

            description = WrapString(description, MAX_CHARACTER_LIMIT);

            string warning = "- WARNING: This program deletes any non-related track files.  " +
                "This means that any file not referenced by a track in the working directory will be deleted!\n";

            warning = Colors.bright + Colors.red + WrapString(warning, MAX_CHARACTER_LIMIT) + Colors.blue + new string('-', MAX_CHARACTER_LIMIT) + '\n' + Colors.normal;

            string extraDescription = "- There is a safeguard in place by creating a backup folder. If you think anything important might've been deleted, you " +
                "can go back and grab it in 'FILE-CLEANER-BACKUP'. At the end of the execution of the program, you will be prompted if you would like to delete " +
                "the backup folder. If you are sure you want to delete the folder you can delete it.\n";

            extraDescription = Colors.bright + WrapString(extraDescription, MAX_CHARACTER_LIMIT) + Colors.normal;

            description += warning;
            description += extraDescription;

            description += Colors.cyan;
            description += new string('-', MAX_CHARACTER_LIMIT);
            description += Colors.normal + '\n';

            Console.WriteLine(description);
        }

        public static void PrintSoftWarning(string text)
        {
            Console.WriteLine(Colors.yellow + $" - Soft warning: {text}" + Colors.normal);
        }

        public static void PrintMessage(string text, string color)
        {
            Console.WriteLine(color + text + Colors.normal);
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
