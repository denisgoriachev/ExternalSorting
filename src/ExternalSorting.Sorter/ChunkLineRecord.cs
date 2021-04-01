namespace ExternalSorting.Sorter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal unsafe struct ChunkLineRecord
    {
        public int ChunkIndex;
        public LineRecord LineRecord;
    }

    internal sealed class ChunkLineRecordComparer : IComparer<ChunkLineRecord>
    {
        public int Compare(ChunkLineRecord x, ChunkLineRecord y)
        {
            return LineRecordComparer.CompareRecords(x.LineRecord, y.LineRecord);
        }
    }
}
