using System;
using System.IO.Enumeration;
using System.Linq;
using System.Text.RegularExpressions;

namespace TrackFileCleaner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserInterface.PromptEnter();
            string[] dirs = Directory.GetDirectories(Environment.CurrentDirectory, "*", SearchOption.TopDirectoryOnly);
            
            foreach (string dir in dirs)
            {
                List<StreamReader> SourceFiles = new List<StreamReader>();
                string[] subdirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);

                // Get any source files in the upper directory
                FileExistsInDir(dir, SourceFiles);

                string BasenameFolder = new DirectoryInfo(dir).Name;
                string BasenameFolderLowered = BasenameFolder.ToLower();

                // Get any source files in the subdirectory
                foreach (string subdir in subdirs)
                {
                    FileExistsInDir(subdir, SourceFiles);
                }

                bool PrintOutline = (SourceFiles.Count > 0);
                if (PrintOutline) UserInterface.PrintOutlinePrompt('-', $"Reading files from {BasenameFolder}...", Colors.cyan);

                // This function will read in all the used source files into Globals.UsedFilePaths
                ReadAllSourceFiles(BasenameFolderLowered, SourceFiles);

                // Now that we have all the used files, go through all the files in every subdirectory and see if it's being used
                AddUnusedFiles(BasenameFolderLowered);

                // After we're done with this folder remove any used files associated with the folder
                RemoveUsedFiles(BasenameFolderLowered);

                if (PrintOutline) UserInterface.PrintOutlinePrompt('-', $"Done grabbing excess files from {BasenameFolder}...", Colors.green, Colors.green);

                // Clear the source files list
                SourceFiles.Clear();
            }

            // Delete all unused files and empty directories
            DeleteAllUnusedFiles();

            Globals.UsedFilePaths.Clear();
            Globals.UnusedFilePaths.Clear();

            UserInterface.PromptClose();
        }

        /// <summary>
        /// Checks if a valid MX Simulator file with valuable information on what files the track uses exists
        /// in directory 'dir' and stores the file in the list 'list'
        /// </summary>
        /// <param name="dir">The directory we are looking through</param>
        /// <param name="list">The list we will append a valid MX Simulator file to</param>
        private static void FileExistsInDir(string dir, List<StreamReader> list)
        {
            foreach (string filename in Globals.ValidMXSimulatorFilenames)
            {
                if (File.Exists(dir + "\\" + filename))
                {
                    list.Add(new StreamReader(dir + "\\" + filename));
                }
            }
        }

        private static void ReadAllSourceFiles(string folder, List<StreamReader> files)
        {
            // Go through all the valid MX Simulator Files
            foreach (StreamReader SourceFile in files)
            {

                string filename = ((FileStream)SourceFile.BaseStream).Name;

                // Skip each line in frills.js for now
                if (filename.EndsWith("frills.js") || filename.EndsWith("nofrills.js"))
                {
                    ParseJavascript(filename, folder);
                    continue;
                }

                while (SourceFile.EndOfStream == false)
                {
                    string line = SourceFile.ReadLine() ?? "";
                    if (!line.Contains('@')) continue;

                    int index = line.IndexOf('@');
                    while (index != -1)
                    {
                        line = line[index..];
                        int SeparatorIndex = line.IndexOf(' ');

                        if (SeparatorIndex == -1)
                        {
                            SeparatorIndex = line.Length;
                        }

                        string SourceFilename = line[1..SeparatorIndex].ToLower();
                        string FullSourcePath = (Environment.CurrentDirectory + "/" + SourceFilename).Replace('/', '\\');

                        // If the filename exists and isn't in the list already add it
                        if (File.Exists(FullSourcePath) && !Globals.UsedFilePaths.Contains(SourceFilename))
                        {
                            // If the item was in the unused files list remove it
                            int UnusedIndex = Globals.UnusedFilePaths.IndexOf(SourceFilename);
                            if (UnusedIndex != -1)
                            {
                                Globals.UnusedFilePaths.RemoveAt(UnusedIndex);
                            }

                            if (!SourceFilename.Contains(folder))
                            {
                                UserInterface.PrintSoftWarning($"File \"{SourceFilename}\" is an outside reference.");
                            }

                            Globals.UsedFilePaths.Add(SourceFilename);

                            // If we have a sequence file we need to add all the sources files in there as well
                            if (Path.GetExtension(SourceFilename) == ".seq")
                            {
                                string filepath = Environment.CurrentDirectory + '\\' + SourceFilename;
                                if (!File.Exists(filepath))
                                {
                                    UserInterface.PrintError($"Cannot Access: {SourceFilename}. File does not exist.");
                                    if (SeparatorIndex == line.Length) break;
                                    index = line.IndexOf('@', SeparatorIndex + 1);
                                    continue;
                                }

                                ReadSeqFiles(filepath, folder, Globals.UsedFilePaths);
                            }
                        }

                        // If the separator index is the line length we've reached the end of the line
                        if (SeparatorIndex == line.Length) break;

                        // Search for the next @ symbol
                        index = line.IndexOf('@', SeparatorIndex + 1);
                    }

                }

                SourceFile.Close();
            }
        }

        private static void RemoveUsedFiles(string folder)
        {
            for (int i = 0; i < Globals.UsedFilePaths.Count; i++)
            {
                if (Globals.UsedFilePaths[i].Contains(folder))
                {
                    Globals.UsedFilePaths.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Gets all the files in a track folder, compares them to see if they're being used in any way, and if they
        /// aren't it will be added to the UnusedFilePaths list.
        /// 
        /// Also handles normal and specular maps. These are not included in the source files, so if the file they reference exists,
        /// it will add the normal and specular map images to the UsedFilePaths list, otherwise it will add it to the UnusedFilePaths list
        /// </summary>
        /// <param name="folder">The track folder</param>
        private static void AddUnusedFiles(string folder)
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + '\\' + folder, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                // Convert a full path to a condensed path starting at the track folder.
                // Ex. C:\\Users\\Jakob\\Desktop\\TrackFolder\\TrackFile.txt converts to: trackfolder/trackfile.txt

                string CondensedFilePath = file[file.IndexOf(folder)..].Replace('\\', '/').ToLower();
                string FileName = Path.GetFileName(CondensedFilePath);

                // If it's an ignore file we will skip
                if (Globals.IgnoreFiles.Contains(FileName)) continue;

                if (!Globals.UsedFilePaths.Contains(CondensedFilePath))
                {
                    // Here we handle norm and specular maps, if the file they reference exists it's added to the UsedFilePaths list
                    bool GotoNextFile = HandleNormsAndSpecs(CondensedFilePath, folder);
                    if (GotoNextFile) continue;

                    Globals.UnusedFilePaths.Add(CondensedFilePath);
                }
            }
        }

        /// <summary>
        /// Reads all the source files in a sequence animation file
        /// </summary>
        /// <param name="filepath">The full path to the file</param>
        /// <param name="BasenameFolder">The name of the track folder</param>
        /// <param name="list">The list we will append to</param>
        /// <param name="WarningMessage">Boolean that determines if we should add a warning message for outside references</param>
        private static void ReadSeqFiles(string filepath, string BasenameFolder, List<string> list, bool WarningMessage = true)
        {
            if (Path.GetExtension(filepath) != ".seq") return;

            StreamReader SeqFile = File.OpenText(filepath);
            while (SeqFile.EndOfStream == false)
            {
                string SeqLine = SeqFile.ReadLine() ?? "";
                if (!SeqLine.Contains('@')) continue;

                int i = 1 + SeqLine.IndexOf('@');
                string SeqSourceFilename = SeqLine[i..].ToLower();

                if (!list.Contains(SeqSourceFilename))
                {
                    int UnusedIndex = Globals.UnusedFilePaths.IndexOf(SeqSourceFilename);
                    if (UnusedIndex != -1)
                    {
                        Globals.UnusedFilePaths.RemoveAt(UnusedIndex);
                    }

                    if (WarningMessage && !SeqSourceFilename.Contains(BasenameFolder))
                    {
                        UserInterface.PrintSoftWarning($"File \"{SeqSourceFilename}\" is an outside reference.");
                    }
                    list.Add(SeqSourceFilename);
                }
            }
            SeqFile.Close();
        }

        /// <summary>
        /// This handles any normal/specular images including sequences and repeats. If we have sequence files,
        /// it will go through the files and add them to the UsedFilePaths list
        /// </summary>
        /// <param name="filepath">the condensed filepath with path/to/folder/trackfile</param>
        /// <param name="folder">the basename of the track folder we're working with</param>
        /// <returns>true: we will continue to the next file | false: add the file the unused files list</returns>
        private static bool HandleNormsAndSpecs(string filepath, string folder)
        {
            bool DecalNorm = filepath.Contains("_tnorm.") || filepath.Contains("_tnorm-repeat.");
            bool DecalSpec = filepath.Contains("_tspec.") || filepath.Contains("_tspec-repeat.");
            if (DecalNorm || DecalSpec)
            {
                // check to see if the decal exists in the used file paths instead
                int index = filepath.IndexOf("_tnorm");
                if (index == -1)
                {
                    index = filepath.IndexOf("_tspec");
                }

                string extension = Path.GetExtension(filepath);

                List<string> SeqFiles = new List<string>();
                string DecalExtension = extension;
                if (extension == ".seq")
                {
                    ReadSeqFiles(filepath, folder, SeqFiles, false);
                    DecalExtension = Path.GetExtension(SeqFiles[0]);
                }

                string DecalName = filepath[..index] + DecalExtension;

                // If the decal associated with this normal or spec is being used continue
                if (Globals.UsedFilePaths.Contains(DecalName))
                {
                    Globals.UsedFilePaths.Add(filepath);

                    // If we have a sequence file we will add it to the UsedFilePaths list since it wouldn't have been added
                    if (extension == ".seq")
                    {
                        foreach (string SeqFile in SeqFiles)
                        {
                            if (!SeqFile.Contains(folder))
                            {
                                UserInterface.PrintSoftWarning($"File \"{SeqFile}\" is an outside reference.");
                            }
                        }

                        Globals.UsedFilePaths.AddRange(SeqFiles);
                    }
                    
                    return true;
                }
            }

            bool StatueNorm = filepath.Contains("_norm.") || filepath.Contains("_norm-repeat.");
            bool StatueSpec = filepath.Contains("_spec.") || filepath.Contains("_spec-repeat.");

            if (StatueNorm || StatueSpec)
            {
                // check to see if the jm exists in the used file paths instead
                int index = filepath.IndexOf("_norm");
                if (index == -1)
                {
                    index = filepath.IndexOf("_spec");
                }

                string repeat = (filepath.Contains("-repeat")) ? "-repeat" : "";

                string extension = Path.GetExtension(filepath);

                string StatueJM = filepath[..index] + ".jm";
                string associatedPNG = filepath[..index] + repeat + extension;

                // if the JM associated with this norm or spec map is being used continue to next file
                if (Globals.UsedFilePaths.Contains(StatueJM) || Globals.UsedFilePaths.Contains(associatedPNG))
                {
                    Globals.UsedFilePaths.Add(filepath);

                    // Add sequence files to UsedFilePaths list
                    if (extension == ".seq")
                    {
                        List<string> SeqFiles = new List<string>();
                        ReadSeqFiles(filepath, folder, SeqFiles);
                        Globals.UsedFilePaths.AddRange(SeqFiles);
                    }
                    
                    return true;
                }
            }
            return false;
        }

        private static void ParseJavascript(string filename, string folder)
        {
            // Read entire Javascript file into string
            string jsCode = File.ReadAllText(filename);

            // regular expression pattern to find lines with potential file references
            string pattern = @"(@[a-zA-Z0-9_\-/[\] ]+\.[a-zA-Z0-9]+)";

            // Use regular expressions to find file matches
            MatchCollection matches = Regex.Matches(jsCode, pattern);

            foreach (Match match in matches)
            {
                string fileReference = match.Groups[1].Value.ToLower()[1..];
                string FullPath = (Environment.CurrentDirectory + "/" + fileReference).Replace('/','\\');

                if (File.Exists(FullPath) && !Globals.UsedFilePaths.Contains(fileReference))
                {
                    int UnusedIndex = Globals.UnusedFilePaths.IndexOf(fileReference);
                    if (UnusedIndex != -1)
                    {
                        Globals.UnusedFilePaths.RemoveAt(UnusedIndex);
                    }

                    if (Path.GetExtension(fileReference) == ".seq")
                    {
                        ReadSeqFiles(FullPath, folder, Globals.UsedFilePaths);
                    }

                    if (!fileReference.Contains(folder))
                    {
                        UserInterface.PrintSoftWarning($"File \"{fileReference}\" is an outside reference.");
                    }
                    Globals.UsedFilePaths.Add(fileReference);
                }
            }

        }

        private static void DeleteAllUnusedFiles()
        {
            UserInterface.PrintOutlinePrompt('#', "Deleting All Unused Files", Colors.cyan);

            int itemsDeleted = 0;

            // Delete every file that's unused
            foreach (string file in Globals.UnusedFilePaths)
            {
                string FullPath = Environment.CurrentDirectory + "/" + file;
                if (!File.Exists(FullPath)) continue;

                try
                {
                    File.Delete(FullPath);
                    itemsDeleted++;
                    UserInterface.PrintMessage($" - Deleted file {file}", Colors.red);
                }
                catch (IOException ex)
                {
                    UserInterface.PrintError($"Error deleting file {file}: {ex.Message}");
                }
            }

            // Delete any folders with zero items in them
            string[] dirs = Directory.GetDirectories(Environment.CurrentDirectory, "*", SearchOption.AllDirectories);
            
            // Sort in descending order by directory length
            Array.Sort(dirs, SortDirectoryByDepth);

            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir);
                string[] subfolders = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);

                int indexToTrimStart = Environment.CurrentDirectory.Length + 1;
                string CondensedDirectory = dir[indexToTrimStart..].Replace('\\','/').ToLower();

                if (files.Length == 0 && subfolders.Length == 0)
                {
                    try
                    {
                        Directory.Delete(dir);
                        itemsDeleted++;
                        UserInterface.PrintMessage($" - Deleted directory {CondensedDirectory}", Colors.red);
                    }
                    catch (IOException ex)
                    {
                        UserInterface.PrintError($"Error deleting directory {dir}: {ex.Message}");
                    }
                }
            }

            if (itemsDeleted == 0)
            {
                UserInterface.PrintMessage(" - No Files/Directories to delete! You're all clean! :)", Colors.green);
            }
        }

        private static int SortDirectoryByDepth(string x, string y)
        {
            int xDepth = x.Split('\\').Length;
            int yDepth = y.Split('\\').Length;
            return yDepth - xDepth;
        }
    }
}