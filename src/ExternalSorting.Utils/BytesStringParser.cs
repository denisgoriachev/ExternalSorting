using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExternalSorting.Utils
{
    public static class BytesStringParser
    {
        public const string KilobyteSuffix = "k";

        public const string MegabyteSuffix = "m";

        public const string GigabyteSuffix = "g";

        private static readonly Regex _bytesRegex = new Regex($"^(\\d+)([{KilobyteSuffix}|{MegabyteSuffix}|{GigabyteSuffix}]){{0,1}}$");

        public static long ParseBytesString(string source)
        {
            var match = _bytesRegex.Match(source);

            if (match == null || match.Groups.Count != 3)
                throw new ArgumentException("Provided string not in valid format. Consider using the following format: [min]-[max] " +
                    "where [number] - any integer and [suffix] - one of the following: \"k\" (kilobytes), \"m\" (megabytes), \"g\" (gigabytes)", nameof(source));

            return int.Parse(match.Groups[1].Value) * SiffixToActualBytes(match.Groups[2].Value);
        }

        private static long SiffixToActualBytes(string value)
        {
            switch (value)
            {
                case KilobyteSuffix:
                    return 1024L;
                case MegabyteSuffix:
                    return 1024L * 1024L;
                case GigabyteSuffix:
                    return 1024L * 1024L * 1024L;
                case "":
                    return 1L;
                default:
                    throw new NotSupportedException($"Provided suffix \"{value}\" does not supported.");
            }
        }
    }
}
