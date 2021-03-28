namespace ExternalSorting.Sorter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [DebuggerDisplay("{NumberPart}. {StringPart}")]
    internal struct LineRecord
    {
        public int NumberPart;

        public string StringPart;
    }

    internal class LineRecordComparer : IComparer<LineRecord>
    {
        public int Compare(LineRecord x, LineRecord y)
        {
            var stringPartComparisonResult = x.StringPart.CompareTo(y.StringPart);
            return stringPartComparisonResult == 0 ? x.NumberPart.CompareTo(y.NumberPart) : stringPartComparisonResult;
        }
    }
}
