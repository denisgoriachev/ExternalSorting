namespace ExternalSorting.Sorter
{
    using ExternalSorting.Utils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class ChunkProducer
    {
        private readonly ConcurrentLineRecordStringReader _lineRecordReader;
        private static readonly object _lock = new object();

        public ChunkProducer(ConcurrentLineRecordStringReader lineRecordReader)
        {
            _lineRecordReader = lineRecordReader;
        }

        public unsafe Task<int> ProduceSortedChunksAsync(long availableRam, int numberOfChunksInParallel, string temporaryFolder, int outputBufferSize, IProgress<string>? progress)
        {
            var tasks = new Task[numberOfChunksInParallel];
            int chunkIndex = 0;

            var lineRecordSize = MemoryHelper.SizeOf<LineRecord>();

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var inMemoryList = new LineRecord[(availableRam / numberOfChunksInParallel / 2 / lineRecordSize) + 1];
                    var comparer = new LineRecordComparer();

                    var buffer = new char[availableRam / numberOfChunksInParallel / sizeof(char) / 2];

                    fixed (char* pBuffer = buffer)
                    {
                        var readedBytes = 0;

                        while ((readedBytes = _lineRecordReader.ReadLineRecordsAsync(buffer).GetAwaiter().GetResult()) != 0)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            var currentChunkIndex = 0;

                            lock (_lock)
                            {
                                currentChunkIndex = chunkIndex;
                                chunkIndex++;
                            }

                            progress?.Report($"Processing chunk {currentChunkIndex} ({readedBytes} bytes)...");

                            var span = new Span<char>(buffer, 0, readedBytes);

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

                            progress?.Report($"Chunk {currentChunkIndex} completed!");
                        }
                    }
                });
            }

            return Task.WhenAll(tasks).ContinueWith(t => chunkIndex);
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
