using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackFileCleaner
{
    internal class Globals
    {
        // The files we will be reading from to gather all source files
        public readonly static string[] ValidMXSimulatorFilenames = new string[] {"billboards", "statues", "decals", "flaggers", 
            "nofrills.js", "frills.js", "lighting", "statues", "tileinfo", "timing_gates"};

        // The files we will not delete
        public readonly static string[] IgnoreFiles = new string[] {"cameras", "desc", "edinfo", "fastlap.mxdemo", "lastlap.mxdemo",
            "lines_amateur","lines_expert", "lines_novice", "map.png", "shading.ppm", "shadingx2.ppm", "shadows.pgm", "terrain.hf",
            "terrain.png", "texturelist", "tilemap"}.Concat(ValidMXSimulatorFilenames).ToArray();

        public readonly static string[] RiderFilesToIgnore = new string[]
        {
            "rider_head", "rider_body", "rider_body_fp",
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

        public readonly static int[] BikeYears = new int[] 
        { 
            2006, 2007, 2008, 2009,
            2011, 2012, 2013, 2016, 2017, 2018
        };


        // The files being used by a track
        public static List<string> UsedFilePaths = new List<string>();

        // The files not being used by a track
        public static List<string> UnusedFilePaths = new List<string>();

        // Non Track Files we will skip
        public static List<string> NonTrackFilePatterns = new List<string>();

        public static void SetupNonTrackFiles()
        {
            for (int i = 0; i < RiderFilesToIgnore.Length; i++)
            {
                NonTrackFilePatterns.Add(RiderFilesToIgnore[i]);
            }

            for (int i = 0; i < BikeFilesToIgnore.Length; i++)
            {
                string bike = BikeFilesToIgnore[i];

                NonTrackFilePatterns.Add(bike);
                if (bike.Contains("_se"))
                {
                    NonTrackFilePatterns.Add($"{bike}v2006");
                    continue;
                }

                if (bike.Contains("rs50") || bike.Contains("125")) continue;

                for (int j = 0; j < BikeYears.Length; j++)
                {
                    int year = BikeYears[j];
                    string BikeYear = $"{bike}v{year}";
                    NonTrackFilePatterns.Add(BikeYear);
                }
            }
        }
    }
}
