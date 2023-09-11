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

        // The files being used by a track
        public static List<string> UsedFilePaths = new List<string>();

        // The files not being used by a track
        public static List<string> UnusedFilePaths = new List<string>();
    }
}
