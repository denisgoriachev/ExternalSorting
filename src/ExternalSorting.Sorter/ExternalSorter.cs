using ExternalSorting.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //MergeSortedChunks(temporaryFolder, outputFile, availableRam, outputBufferSize, progress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PrepareSortedChunks(string inputFile, string temporaryFolder, long availableRam, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            var lineRecordSize = MemoryHelper.SizeOf<LineRecord>();

            // allocating array which will be in limits of allowed memory usage always (even in worst case when all string parts are empty)
            // Division by 2 required because of sorting - each sorting algorithm might require O(n) space
            var inMemoryList = new LineRecord[(availableRam / 2 / lineRecordSize) + 1];

            var comparer = new LineRecordComparer();
            //int currentChunkIndex = 0;

            unsafe
            {
                var buffer = new char[availableRam / sizeof(char) / 2];
                fixed (char* pBuffer = buffer)
                {
                    var bufferOffset = 0;
                    var currentChunkIndex = 0;
                    var readedBytes = 0;

                    using (var inputReader = new StreamReader(inputFile, Encoding.UTF8, true, inputBufferSize))
                    {
                        while ((readedBytes = inputReader.Read(buffer, bufferOffset, buffer.Length - bufferOffset)) != 0)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            progress?.Report($"Processing chunk {currentChunkIndex} ({buffer.Length - bufferOffset} bytes)...");

                            var span = new Span<char>(buffer, 0, bufferOffset + readedBytes);

                            bufferOffset = 0;
                            var inMemoryListIndex = 0;
                            var memoryOffset = 0;

                            while (span.Length > 0)
                            {
                                var dotIndex = span.IndexOf('.');
                                var newLineIndex = span.IndexOf(Environment.NewLine);

                                if (dotIndex == -1 || newLineIndex == -1)
                                {

                                    break;
                                }


                                inMemoryList[inMemoryListIndex].NumberPart = FastParse(span.Slice(0, dotIndex));

                                inMemoryList[inMemoryListIndex].StringPart = &pBuffer[memoryOffset + dotIndex + 2];
                                inMemoryList[inMemoryListIndex].StringPartLength = newLineIndex - dotIndex - 2;
                                inMemoryListIndex++;

                                var nextLineStart = newLineIndex + Environment.NewLine.Length;
                                memoryOffset += nextLineStart;
                                span = span.Slice(nextLineStart);
                            }

                            progress?.Report($"Sorting chunk {currentChunkIndex} ...");
                            Array.Sort(inMemoryList, 0, inMemoryListIndex, comparer);

                            progress?.Report($"Writing chunk {currentChunkIndex}...");

                            using (var fileStream = new FileStream(Path.Combine(temporaryFolder, $"chunk.{currentChunkIndex}.bin"), FileMode.Create, FileAccess.Write, FileShare.None, outputBufferSize))
                            using (var binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                            {
                                binaryWriter.Write(inMemoryListIndex);
                                for (var i = 0; i < inMemoryListIndex; i++)
                                {
                                    var lineRecord = inMemoryList[i];
                                    binaryWriter.Write(lineRecord.NumberPart);
                                    binaryWriter.Write(lineRecord.StringPartLength);

                                    for (int j = 0; j < lineRecord.StringPartLength; j++)
                                    {
                                        binaryWriter.Write(lineRecord.StringPart[j]);
                                    }
                                }
                            }


                            currentChunkIndex++;

                            span.CopyTo(new Span<char>(buffer));
                            bufferOffset = span.Length;
                        }
                    }

                    return currentChunkIndex;
                }
            }
        }

        //static void MergeSortedChunks(string temporaryFolder, string outputFile, long availableRam, int outputBufferSize, IProgress<string>? progress)
        //{
        //    var files = Directory.GetFiles(temporaryFolder, "chunk.*.bin");

        //    var chunks = new List<ChunkReaderQueue>(files.Length);

        //    foreach (var file in files)
        //    {
        //        var chunk = new ChunkReaderQueue(file, (int)(availableRam / files.Length));
        //        chunks.Add(chunk);
        //    }

        //    var comparer = new LineRecordComparer();

        //    using (var streamWriter = new StreamWriter(outputFile, false, Encoding.UTF8, outputBufferSize))
        //    {
        //        while (chunks.Count > 1)
        //        {
        //            LineRecord? minLineRecord = null;

        //            var minLineRecordChunkIndex = 0;

        //            for (var i = 0; i < chunks.Count; i++)
        //            {
        //                var chunk = chunks[i];

        //                if (chunk.IsEmpty)
        //                {
        //                    chunks.RemoveAt(i);
        //                    chunk.Dispose();
        //                    i--;
        //                    continue;
        //                }

        //                if (minLineRecord == null)
        //                {
        //                    minLineRecord = chunk.Peek();
        //                }

        //                var currentChunkRecord = chunk.Peek();

        //                if (comparer.Compare(minLineRecord.Value, currentChunkRecord) > 0)
        //                {
        //                    minLineRecord = currentChunkRecord;
        //                    minLineRecordChunkIndex = i;
        //                }
        //            }

        //            if (minLineRecord == null)
        //                continue;

        //            streamWriter.Write(minLineRecord.Value.NumberPart);
        //            streamWriter.Write(". ");
        //            streamWriter.WriteLine(minLineRecord.Value.StringPart);

        //            chunks[minLineRecordChunkIndex].Dequeue();
        //        }

        //        var lastChunk = chunks[0];

        //        while (!lastChunk.IsEmpty)
        //        {
        //            var lineRecord = lastChunk.Peek();

        //            streamWriter.Write(lineRecord.NumberPart);
        //            streamWriter.Write(". ");
        //            streamWriter.WriteLine(lineRecord.StringPart);

        //            lastChunk.Dequeue();
        //        }

        //        lastChunk.Dispose();
        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteChunk(string temporayFolderPath, int chunkIndex, LineRecord[] records, int count, int bufferSize)
        {

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
