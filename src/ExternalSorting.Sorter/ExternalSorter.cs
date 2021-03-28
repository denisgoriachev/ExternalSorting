using ExternalSorting.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    public static class ExternalSorter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(string inputFile, string outputFile, string temporaryFolder, long availableRam, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            var actualNumberOfChunks = PrepareSortedChunks(inputFile, temporaryFolder, availableRam, inputBufferSize, outputBufferSize, progress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PrepareSortedChunks(string inputFile, string temporaryFolder, long availableRam, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            var lineRecordSize = MemoryHelper.SizeOf<LineRecord>();

            // allocating array which will be in limits of allowed memory usage always (even in worst case when all string parts are empty)
            // Division by 2 required because of sorting - each sorting algorithm might require O(n) space
            var inMemoryList = new LineRecord[(availableRam / lineRecordSize) + 1];

            var comparer = new LineRecordComparer();
            //int currentChunkIndex = 0;

            var buffer = new char[availableRam / sizeof(char) / 2];

            using (var inputReader = new StreamReader(inputFile, Encoding.UTF8, true, inputBufferSize))
            {
                var bufferOffset = 0;
                var currentChunkIndex = 0;
                var readedBytes = 0;

                while ((readedBytes = inputReader.Read(buffer, bufferOffset, buffer.Length - bufferOffset)) != 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    progress?.Report($"Processing chunk {currentChunkIndex} ({buffer.Length - bufferOffset} bytes)...");
                    var memory = new Memory<char>(buffer, 0, bufferOffset + readedBytes);

                    var span = memory.Span;

                    bufferOffset = 0;
                    var inMemoryListIndex = 0;
                    var memoryOffset = 0;

                    while (span.Length > 0)
                    {
                        var dotIndex = span.IndexOf('.');
                        var newLineIndex = span.IndexOf(Environment.NewLine);

                        if (dotIndex == -1 || newLineIndex == -1)
                        {
                            span.CopyTo(memory.Span);
                            bufferOffset = span.Length;
                            break;
                        }


                        inMemoryList[inMemoryListIndex].NumberPart = FastParse(span.Slice(0, dotIndex));
                        inMemoryList[inMemoryListIndex].StringPart = memory.Slice(memoryOffset + dotIndex + 2, newLineIndex - dotIndex - 2);
                        inMemoryListIndex++;

                        var nextLineStart = newLineIndex + Environment.NewLine.Length;
                        memoryOffset += nextLineStart;
                        span = span.Slice(nextLineStart);
                    }

                    progress?.Report($"Sorting chunk {currentChunkIndex} ...");
                    Array.Sort(inMemoryList, 0, inMemoryListIndex, comparer);

                    progress?.Report($"Writing chunk {currentChunkIndex}...");
                    WriteChunk(temporaryFolder, currentChunkIndex, inMemoryList, inMemoryListIndex, outputBufferSize);

                    currentChunkIndex++;
                }

                return currentChunkIndex;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteChunk(string temporayFolderPath, int chunkIndex, LineRecord[] records, int count, int bufferSize)
        {
            using (var fileStream = new FileStream(Path.Combine(temporayFolderPath, $"chunk.{chunkIndex}.bin"), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize))
            using (var binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
            {
                binaryWriter.Write(count);
                for (var i = 0; i < count; i++)
                {
                    var lineRecord = records[i];
                    binaryWriter.Write(lineRecord.NumberPart);
                    binaryWriter.Write(lineRecord.StringPart.Span);
                    lineRecord.StringPart = ReadOnlyMemory<char>.Empty;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int FastParse(Span<char> source)
        {
            var index = source.Length - 1;
            var result = source[index] - 0x30;
            index--;
            var radix = 10;
            while (index >= 0)
            {
                result += (source[index] - 0x30) * radix;
                radix *= 10;
                index--;
            }

            return result;
        }
    }
}
