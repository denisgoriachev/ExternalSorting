using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    [DebuggerDisplay("{NumberPart}. {StringPart}")]
    internal struct LineRecord
    {
        public int NumberPart;

        public ReadOnlyMemory<char> StringPart;
    }

    internal class LineRecordComparer : IComparer<LineRecord>
    {
        public int Compare(LineRecord x, LineRecord y)
        {
            var stringPartComparisonResult = x.StringPart.Span.CompareTo(y.StringPart.Span, StringComparison.Ordinal);
            return stringPartComparisonResult == 0 ? x.NumberPart.CompareTo(y.NumberPart) : stringPartComparisonResult;
        }
    }
}
