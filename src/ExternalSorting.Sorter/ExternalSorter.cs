using ExternalSorting.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    public static class ExternalSorter
    {
        [DebuggerDisplay("{NumberPart}. {StringPart}")]
        struct LineRecord
        {
            public int NumberPart;
            public string StringPart;
        }

        class LineRecordComparer : IComparer<LineRecord>
        {
            public int Compare(LineRecord x, LineRecord y)
            {
                var stringPartComparisonResult = x.StringPart.CompareTo(y.StringPart);
                return stringPartComparisonResult == 0 ? x.NumberPart.CompareTo(y.NumberPart) : stringPartComparisonResult;
            }
        }

        public static void Sort(string inputFile, string outputFile, string temporaryFolder, long memoryUsage, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            var lineRecordSize = MemoryHelper.SizeOf<LineRecord>();

            // allocating array which will be in limits of allowed memory usage always (even in worst case when all string parts are empty)
            // Division by 2 required because of sorting - each sorting algorithm might require O(n) space
            var inMemoryList = new LineRecord[(memoryUsage / (lineRecordSize + sizeof(long))) / 2];
            long consumedMemory = 0;

            var comparer = new LineRecordComparer();
            int currentChunkIndex = 0;

            using (var inputReader = new StreamReader(inputFile, Encoding.UTF8, true, inputBufferSize))
            {
                string? line = null;
                var readedLines = 0;

                while ((line = inputReader.ReadLine()) != null)
                {
                    var span = line.AsSpan();

                    var index = 0;
                    while (span[index] != '.')
                    {
                        index++;
                    }

                    inMemoryList[readedLines].NumberPart = int.Parse(span.Slice(0, index));
                    inMemoryList[readedLines].StringPart = new string(span.Slice(index + 2));

                    readedLines++;

                    consumedMemory += lineRecordSize + sizeof(long) + (span.Length - index - 2) * 2;

                    if (consumedMemory >= memoryUsage || readedLines == inMemoryList.Length)
                    {
                        Array.Sort(inMemoryList, 0, readedLines, comparer);

                        WriteChunk(temporaryFolder, currentChunkIndex, inMemoryList, readedLines, outputBufferSize);

                        currentChunkIndex++;
                        consumedMemory = 0;
                    }
                }
            }
        }

        static void WriteChunk(string temporayFolderPath, int chunkIndex, LineRecord[] records, int count, int bufferSize)
        {
            using (var fileStream = new FileStream(Path.Combine(temporayFolderPath, chunkIndex.ToString()), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize))
            using (var binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
            {
                binaryWriter.Write(count);
                for (var i = 0; i < count; i++)
                {
                    var lineRecord = records[i];
                    binaryWriter.Write(lineRecord.NumberPart);
                    binaryWriter.Write(lineRecord.StringPart);
                }
            }
        }
    }
}
