using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    public class SorterOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input filename.")]
        public string InputFile { get; set; } = string.Empty;

        [Option('o', "output", Required = true, HelpText = "Output filename.")]
        public string OutputFile { get; set; } = string.Empty;

        [Option('t', "temp-folder", Required = false, HelpText = "Temporary folder where intermidiate results will be stored. If empty - OS temporary folder will be used.")]
        public string TemporaryFolder { get; set; } = string.Empty;

        [Option('c', "clean-temp-folder", HelpText = "Flag indicates that temporary folder should be cleaned up after sorting.")]
        public bool CleanTemporaryFolder { get; set; }

        [Option('m', "-ram", Required = true, HelpText = "Desired RAM usage for external sorting in format [number][suffix], where [number] - positive integer, " +
           "[suffix] - one of the following: \"k\" (kilobytes), \"m\" (megabytes), \"g\" (gigabytes) or empty (for bytes).")]
        public string AvailableRam { get; set; } = "1g";

        [Option('r', "read-buffer-size", Required = true, HelpText = "Buffer size for reading data from disk in format [number][suffix], where [number] - positive integer, " +
           "[suffix] - one of the following: \"k\" (kilobytes), \"m\" (megabytes), \"g\" (gigabytes) or empty (for bytes).")]
        public string ReadBufferSize { get; set; } = "16m";

        [Option('w', "write-buffer-size", Required = true, HelpText = "Buffer size for writing data to disk in format [number][suffix], where [number] - positive integer, " +
           "[suffix] - one of the following: \"k\" (kilobytes), \"m\" (megabytes), \"g\" (gigabytes) or empty (for bytes).")]
        public string WriteBufferSize { get; set; } = "16m";
    }
}
