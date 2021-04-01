namespace ExternalSorting.Sorter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    [DebuggerDisplay("{NumberPart}. {StringPart}")]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal unsafe struct LineRecord
    {
        public int NumberPart;
        public int StringPartLength;
        public char* StringPart;
    }

    internal sealed class LineRecordComparer : IComparer<LineRecord>
    {
        public unsafe int Compare(LineRecord x, LineRecord y)
        {
            return CompareRecords(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int CompareRecords(LineRecord x, LineRecord y)
        {
            var stringPartComparisonResult = CompareCharArrays(x.StringPart, x.StringPartLength, y.StringPart, y.StringPartLength);
            return stringPartComparisonResult == 0 ? x.NumberPart.CompareTo(y.NumberPart) : stringPartComparisonResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe int CompareCharArrays(char* left, int leftLength, char* right, int rightLength)
        {
            var result = 0;

            var comparisonLerngth = Math.Min(leftLength, rightLength);

            for (var i = 0; i < leftLength; i++)
            {
                if ((result = left[i].CompareTo(right[i])) != 0)
                    return result;
            }

            if (leftLength < rightLength)
            {
                return -1;
            }
            else if (leftLength > rightLength)
            {
                return 1;
            }

            return 0;
        }
    }
}
