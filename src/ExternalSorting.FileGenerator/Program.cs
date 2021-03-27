using CommandLine;
using ExternalSorting.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalSorting.FileGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<FileGeneratorOptions>(args)
                .WithParsed(options =>
                {
                    IProgress<string> progress = new Progress<string>(message => Console.WriteLine(message));

                    try
                    {
                        RunFileGenerator(options, progress);
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"Error: {ex.Message}");
                    }

                    progress.Report("Press any key to continue...");

                    Console.ReadKey();
                });
        }

        static void RunFileGenerator(FileGeneratorOptions options, IProgress<string> progress)
        {
            var random = new Random(options.Seed);

            var fileSize = BytesStringParser.ParseBytesString(options.FileSize ?? throw new ArgumentNullException("File Size cannot be null."));
            var bufferSize = BytesStringParser.ParseBytesString(options.BufferSize ?? throw new ArgumentNullException("Buffer Size cannot be null."));
            var range = RangeParser.Parse(options.Range);

            if (bufferSize > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"Buffer size cannot be greater then {int.MaxValue} bytes.");
            }

            var stopwatch = Stopwatch.StartNew();
            progress.Report($"Starting random file data generation at {DateTime.Now}...");

            RandomFileGenerator.GenerateFile(
                options.OutputFile,
                random,
                options.PossibleStrings.ToArray(),
                range,
                fileSize,
                (int)bufferSize,
                progress);

            stopwatch.Stop();
            progress.Report($"File generated at {DateTime.Now}. Elapsed time: {stopwatch.Elapsed}");
        }
    }
}
