﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DuetControlServer
{
    /*static*/ class Settings
    {
        private static readonly string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        private static readonly RegexOptions RegexFlags =  RegexOptions.IgnoreCase | RegexOptions.Singleline;

        [JsonProperty]
        public static string SocketPath { get; set; } = "/tmp/duet.sock";               // Path to the UNIX socket for IPC

        [JsonProperty]
        public static int Backlog { get; set; } = 4;                                    // Maximum number of pending IPC connections

        [JsonProperty]
        public static string BaseDirectory { get; set; } = "/boot";                     // Directory holding the RepRapFirmware SD card files

        [JsonProperty]
        public static uint FileInfoReadLimit { get; set; } = 32768;                     // How many bytes to parse max at the beginning and end of a file to retrieve G-code file information

        [JsonProperty]
        public static double MaxLayerHeight { get; set; } = 0.9;                        // Maximum allowed layer height

        [JsonProperty]
        public static List<Regex> LayerHeightFilters { get; set; } = new List<Regex>    // Regular expressions for finding the layer height
        {
            new Regex(@"layer_height\D+(?<mm>(\d+\.?\d*))", RegexFlags),                // Slic3r
            new Regex(@"Layer height\D+(?<mm>(\d+\.?\d*))", RegexFlags),                // Cura
            new Regex(@"layerHeight\D+(?<mm>(\d+\.?\d*))", RegexFlags),                 // Simplify3D
            new Regex(@"layer_thickness_mm\D+(?<mm>(\d+\.?\d*))", RegexFlags),          // KISSlicer
            new Regex(@"layerThickness\D+(?<mm>(\d+\.?\d*))", RegexFlags)               // Matter Control
        };

        [JsonProperty]
        public static List<Regex> FilamentFilters { get; set; } = new List<Regex>       // Regular expressions for finding the filament consumption (case insensitive, single line)
        {
            new Regex(@"filament used\D+(((?<mm>\d+\.?\d*)mm)(\D+)?)+", RegexFlags),        // Slic3r (mm)
            new Regex(@"filament used\D+(((?<m>\d+\.?\d*)m([^m]|$))(\D+)?)+", RegexFlags),  // Cura (m)
            new Regex(@"material\#\d+\D+(?<mm>\d+\.?\d*)", RegexFlags),                     // IdeaMaker (mm)
            new Regex(@"filament length\D+(((?<mm>\d+\.?\d*)\s*mm)(\D+)?)+", RegexFlags)    // Simplify3D (mm)
        };

        [JsonProperty]
        public static List<Regex> GeneratedByFilters { get; set; } = new List<Regex>    // Regular expressions for finding the slicer (case insensitive)
        {
            new Regex(@"generated by\s+(.+)", RegexFlags),                              // Slic3r and Simplify3D
            new Regex(@";\s*Sliced by\s+(.+)", RegexFlags),                             // IdeaMaker
            new Regex(@";\s*(KISSlicer.*)", RegexFlags),                                // KISSlicer
            new Regex(@";\s*Sliced at:\s*(.+)", RegexFlags),                            // Cura (old)
            new Regex(@";\s*Generated with\s*(.+)", RegexFlags)                         // Cura (new)
        };

        [JsonProperty]
        public static List<Regex> PrintTimeFilters { get; set; } = new List<Regex>      // Regular expressions for finding the print time
        {
            new Regex(@"estimated printing time = ((?<h>(\d+))h\s*)?((?<m>(\d+))m\s*)?((?<s>(\d+))s)?", RegexFlags),                                     // Slic3r PE
            new Regex(@";TIME:(?<s>(\d+\.?\d*))", RegexFlags),                                                                                           // Cura
            new Regex(@"Build time: ((?<h>\d+) hours\s*)?((?<m>\d+) minutes\s*)?((?<s>(\d+) seconds))?", RegexFlags),                                    // Simplify3D
            new Regex(@"Estimated Build Time:\s+((?<h>(\d+\.?\d*)) hours\s*)?((?<m>(\d+\.?\d*)) minutes\s*)?((?<s>(\d+\.?\d*)) seconds)?", RegexFlags)   // KISSlicer
        };

        [JsonProperty]
        public static List<Regex> SimulatedTimeFilters { get; set; } = new List<Regex>
        {
            new Regex(@"; Simulated print time\D+(?<s>(\d+\.?\d*))", RegexFlags)
        };

        // Load the settings from the config file or create it
        internal static void Load()
        {
            if (System.IO.File.Exists(ConfigFile))
            {
                string fileContent = System.IO.File.ReadAllText(ConfigFile);
                JsonConvert.DeserializeObject<Settings>(fileContent);
            }
            else
            {
                string defaultSettings = JsonConvert.SerializeObject(new Settings());
                System.IO.File.WriteAllText(ConfigFile, defaultSettings);
            }
        }

        // Parse override parameters from the command line arguments
        internal static void ParseParameters(string[] args)
        {
            string lastArg = null;
            foreach (string arg in args)
            {
                if (arg == "-h" || arg == "--help")
                {
                    Console.WriteLine("-h, --help: Display this reference");
                    Console.WriteLine("-S, --socket: Specify the UNIX socket path");
                    Console.WriteLine("-b, --base-directory: Set the base path for system and G-code files");
                }
                else if (lastArg == "-S" || lastArg == "--socket")
                {
                    SocketPath = arg;
                }
                else if (lastArg == "-b" || lastArg == "--base-directory")
                {
                    BaseDirectory = arg;
                }
                lastArg = arg;
            }
        }
    }
}
