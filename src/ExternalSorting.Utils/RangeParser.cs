using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExternalSorting.Utils
{
    public static class RangeParser
    {
        private static readonly Regex _rangeRegex = new Regex(@"^(\d+)-(\d+)$");

        public static Range Parse(string value)
        {
            var match = _rangeRegex.Match(value);

            if (match == null || match.Groups.Count != 3)
                throw new ArgumentException("Provided string not in valid format. Consider using the following format: [min]-[max]", nameof(value));

            return new Range(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }
    }
}
