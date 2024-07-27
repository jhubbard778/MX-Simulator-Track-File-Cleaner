using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackFileCleaner
{
    internal class Globals
    {
        // The files we will be reading from to gather all source files
        public readonly static string[] ValidMXSimulatorFilenames = new string[] {"billboards", "decals", "flaggers",
            "nofrills.js", "frills.js", "lighting", "statues", "tileinfo", "timing_gates"};

        // The files we will not delete
        public readonly static string[] IgnoreFiles = new string[] {"cameras", "desc", "edinfo", "fastlap.mxdemo", "lastlap.mxdemo",
            "lines_amateur","lines_expert", "lines_novice", "map.png", "shading.ppm", "shadingx2.ppm", "shadows.pgm", "terrain.hf",
            "terrain.png", "texturelist", "tilemap"}.Concat(ValidMXSimulatorFilenames).ToArray();

        public readonly static string[] IgnoreFolders = new string[] { "demos", "keycam", "series", "setups", "outgoing", "reshade-shaders" };

        public readonly static string[] RiderFilesToIgnore = new string[]
        {
            "rider_head", "rider_body_fp", "rider_body",
            "wheels", "front_wheel", "rear_wheel"
        };

        public readonly static string[] BikeFilesToIgnore = new string[]
        {
            "rs50cr", "rs50kx", "rs50rm", "rs50yz",
            "125sx", "cr125", "kx125", "rm125", "yz125",
            "250sx", "cr250", "kx250", "rm250", "yz250",
            "250sxf", "crf250", "fc250", "kx250f", "rmz250", "yz250f", "yz250f_se",
            "350sxf", "450sxf", "crf450", "fc450", "kx450f", "rmz450", "yz450f",
        };

        public readonly static string BACKUP_FOLDER = Environment.CurrentDirectory + "\\FILE-CLEANER-BACKUP";

        public readonly static int[] sxBikeYears = new int[] { 2013, 2016 };
        public readonly static int[] crBikeYears = new int[] { 2010, 2013, 2014, 2017, 2018 };
        public readonly static int[] kxBikeYears = new int[] { 2013, 2016, 2017 };
        public readonly static int[] rmBikeYears = new int[] { 2010, 2018 };
        public readonly static int[] yzBikeYears = new int[] { 2010, 2014 };

        // The files being used by a track
        public static List<string> UsedFilePaths = new List<string>();

        // The files not being used by a track
        public static List<string> UnusedFilePaths = new List<string>();

        // Non Track Files we will skip
        public static List<string> BikesToIgnoreList = new List<string>();

        public static void SetupNonTrackFiles()
        {
            for (int i = 0; i < BikeFilesToIgnore.Length; i++)
            {
                string bike = BikeFilesToIgnore[i];
                BikesToIgnoreList.Add(bike);

                // Skip years for 50s, 125s, 2 strokes, and special editions
                if (bike.Contains("rs50") || bike.Contains("125") || !bike.Contains('f') || bike.Contains("_se")) continue;

                if (bike.Contains("fc"))
                {
                    BikesToIgnoreList.Add($"{bike}v2016");
                    continue;
                }

                int[] bikeYears = sxBikeYears;
                string[] keys = new string[] { "cr", "kx", "rm", "yz" };
                string? sKeyResult = keys.FirstOrDefault<string>(s => bike.Contains(s));

                switch (sKeyResult)
                {
                    case "cr":
                        bikeYears = crBikeYears;
                        break;
                    case "kx":
                        bikeYears = kxBikeYears;
                        break;
                    case "rm":
                        bikeYears = rmBikeYears; 
                        break;
                    case "yz":
                        bikeYears = yzBikeYears;
                        break;
                }

                for (int j = 0; j < bikeYears.Length; j++)
                {
                    int year = bikeYears[j];
                    string BikeYear = $"{bike}v{year}";
                    BikesToIgnoreList.Add(BikeYear);
                }
            }

            // Sort the list so in descending order by length so we check for years specification first
            BikesToIgnoreList.Sort((x, y) => y.Length.CompareTo(x.Length));
        }
    }
}
