using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.FileGenerator
{
    public class FileGeneratorOptions
    {
        [Option('o', "output", Required = true, HelpText = "Output filename.")]
        public string OutputFile { get; set; } = string.Empty;

        [Option('s', "seed", Required = true, HelpText = "Seed for generating random data.")]
        public int Seed { get; set; }

        [Option('p', "possible-strings", Required = true, HelpText = "Possible strings which can be used in file generator, separated by comma.")]
        public IEnumerable<string> PossibleStrings { get; set; } = Enumerable.Empty<string>();

        [Option('r', "range", Required = true, HelpText = "Numbers range which will be used for every line in format [min]-[max]")]
        public string Range { get; set; } = "0-9999";

        [Option('f', "file-size", Required = true, HelpText = "Desired file size in format [number][suffix], where [number] - positive integer, " +
            "[suffix] - one of the following: \"k\" (kilobytes), \"m\" (megabytes), \"g\" (gigabytes) or emptyor empty (for bytes).")]
        public string FileSize { get; set; } = "1g";

        [Option('b', "buffer-size", Default = "8m", HelpText = "Buffer size for writing data to disk in format [number][suffix], where [number] - positive integer, " +
            "[suffix] - one of the following: \"k\" (kilobytes), \"m\" (megabytes), \"g\" (gigabytes) or empty (for bytes). Consider using optimal buffer side according to the hdd/ssd capabilities.")]
        public string BufferSize { get; set; } = "8m";
    }
}
