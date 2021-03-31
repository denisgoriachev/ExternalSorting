using CommandLine;
using ExternalSorting.Utils;
using System;
using System.Diagnostics;
using System.IO;

namespace ExternalSorting.Sorter
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SorterOptions>(args)
                .WithParsed(options =>
                {
                    IProgress<string> progress = new Progress<string>(message => Console.WriteLine($"{DateTime.Now} | {message}"));

                    try
                    {
                        RunExternalSorting(options, progress);
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"Error: {ex.Message}");
                    }

                    progress.Report("Press any key to continue...");

                    Console.ReadKey();
                });
        }

        static void RunExternalSorting(SorterOptions options, IProgress<string> progress)
        {
            var ram = BytesStringParser.ParseBytesString(options.AvailableRam);
            var readBufferSize = BytesStringParser.ParseBytesString(options.ReadBufferSize);
            var writeBufferSize = BytesStringParser.ParseBytesString(options.WriteBufferSize);

            if (readBufferSize > int.MaxValue)
                throw new ArgumentOutOfRangeException($"Read buffer size cannot be greater then {int.MaxValue} bytes.");

            if (writeBufferSize > int.MaxValue)
                throw new ArgumentOutOfRangeException($"Write buffer size cannot be greater then {int.MaxValue} bytes.");

            var stopwatch = Stopwatch.StartNew();
            progress.Report($"Starting external sorting at {DateTime.Now}...");

            string temporaryFolder = "";

            if (string.IsNullOrWhiteSpace(options.TemporaryFolder))
            {
                temporaryFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }
            else
            {
                temporaryFolder = Path.Combine(options.TemporaryFolder, Guid.NewGuid().ToString());
            }

            Directory.CreateDirectory(temporaryFolder);

            ExternalSorter.Sort(options.InputFile, options.OutputFile, temporaryFolder, ram, options.NumberOfChunks, (int)readBufferSize, (int)writeBufferSize, progress);

            if (options.CleanTemporaryFolder)
                Directory.Delete(temporaryFolder, true);

            stopwatch.Stop();
            progress.Report($"External sorting completed at {DateTime.Now}. Elapsed time: {stopwatch.Elapsed}");
        }
    }
}
