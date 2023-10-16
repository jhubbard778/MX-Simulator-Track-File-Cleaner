# MX Simulator Track File Cleaner Program

## Description
This program is meant to clean up any files not being used by tracks in your MX Simulator Personal Folder / Install Folder.

## Dependencies
You will need .NET Runtime 7.0 or greater to be able to execute this program. You can download .NET Runtime 7.0 on Microsoft's official [download](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) page. Further instructions can be found [here](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net70).

## Instructions
1. Download the [Zip](https://github.com/jhubbard778/MX-Simulator-Track-File-Cleaner/archive/refs/heads/master.zip) and extract the Track File Cleaner Folder.

2. Move TrackFileCleaner.exe to either your MX Simulator Install Folder or MX Simulator Personal Folder.  If you want to single out just one track, make a copy of it and put both the application and track into an empty new folder.

3. Make sure you have .NET Runtime 7.0 or greater installed on your pc.

4. Start TrackFileCleaner.exe

5. You'll be prompted with a warning and exactly what this application will do. You then can press enter and execute the program.

6. At the end of the execution, you can choose if you want to delete the FILE-CLEANER-BACKUP folder. This will delete any and all files/directories inside this folder. If you are sure nothing important was lost, you can safely remove it. **Note:** *This does not move the files to your recycle bin, it will permanently remove it from your pc.*

## False Positive Virus Detection
This program deals with the process of moving files / deleting empty directories on your computer. For that reason it might get marked as a virus.  This is a false positive and can be ignored.  You can verify the process of moving / deleting by looking at the source code found [here](https://github.com/jhubbard778/MX-Simulator-Track-File-Cleaner/blob/master/TrackFileCleaner/Program.cs#L467).

## Notes
- Any files not being used by tracks will be marked to be moved to the backup folder labeled ``FILE-CLEANER-BACKUP`` that is made in the directory of your application.

- This program attempts to skip over any rider/bike skins as well as folders that are deemed important to the functionality of the game.  More details about what constitutes a file skip below.

- Javascript items are found in if it finds a simple string with a filepath starting with the @ symbol. Ex. `var sound = "@TrackFolder/sounds/ambient.sw"`.  If there are any variables in the string it will be skipped and marked an unused file. Ex. `var folder = TrackFolder; var sound = "@" + folder + "/sounds/ambient.sw"` will be skipped because I did not feel like writing an entire JS interpreter.

## Files Skipped
**Obviously this program will keep any important track files such as:**
- billboards
- decals
- flaggers
- nofrills.js
- frills.js
- statues
- tileinfo
- etc...

**It will also attempt to find rider skin files that start with the following strings:**
- rider_head
- rider_body_fp
- rider_body
- wheels
- front_wheel
- rear_wheel

**And it will finally check for bike skins that start with the following strings:**
- rs50rm
- 125sx
- 250sxf
- 250sxfv2013
- 250sxfv2016
- fc450v2016
- etc...

It will check all available years and dyno skin combinations.

## Folders Skipped
**Any file in these directories will be skipped:**
- demos
- keycam
- series
- setups
- outgoing