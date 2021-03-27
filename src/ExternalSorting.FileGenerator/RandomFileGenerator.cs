using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.FileGenerator
{
    public static class RandomFileGenerator
    {
        public static void GenerateFile(string filename, Random random, string[] possibleStrings, Range possibleNumbersRange, long desiredFileSize, int bufferSize, IProgress<string>? progress = null)
        {
            if (possibleStrings.Length == 0)
                throw new ArgumentException("Possible strings should contain at least 1 value.", nameof(possibleStrings));

            if (desiredFileSize <= 0)
                throw new ArgumentException("File size should be greater then 0.", nameof(desiredFileSize));

            using (var stream = new StreamWriter(filename, false, Encoding.UTF8, bufferSize))
            {
                var totalFileLength = 0L;

                var separator = ". ";

                while (totalFileLength < desiredFileSize)
                {
                    var outputNumber = random.Next(possibleNumbersRange.Start.Value, possibleNumbersRange.End.Value).ToString();
                    var outputString = possibleStrings[random.Next(possibleStrings.Length)];

                    stream.Write(outputNumber);
                    stream.Write(separator);
                    stream.WriteLine(outputString);

                    totalFileLength += (outputNumber.Length + separator.Length + outputString.Length + Environment.NewLine.Length);
                }
            }
        }
    }
}
