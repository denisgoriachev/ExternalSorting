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
        public static void Sort(string inputFile, string outputFile, string temporaryFolder, long availableRam, int numberOfChunksInParallel, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            var actualNumberOfChunks = PrepareSortedChunks(inputFile, temporaryFolder, availableRam, numberOfChunksInParallel, inputBufferSize, outputBufferSize, progress);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            MergeSortedChunks(temporaryFolder, outputFile, inputBufferSize, outputBufferSize, progress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PrepareSortedChunks(string inputFile, string temporaryFolder, long availableRam, int numberOfChunksInParallel, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            using (var lineRecordReader = new ConcurrentLineRecordStringReader(inputFile, Encoding.UTF8, true, inputBufferSize))
            {
                var chunkProducer = new ChunkProducer(lineRecordReader);
                unsafe
                {
                    return chunkProducer.ProduceSortedChunksAsync(availableRam, numberOfChunksInParallel, temporaryFolder, outputBufferSize, progress)
                        .GetAwaiter()
                        .GetResult();
                }
            }
        }

        unsafe static void MergeSortedChunks(string temporaryFolder, string outputFile, int inputBufferSize, int outputBufferSize, IProgress<string>? progress)
        {
            progress?.Report("Starting to k-merge chunks...");

            var files = Directory.GetFiles(temporaryFolder, "chunk.*.bin");

            var chunks = new Dictionary<int, ChunkReaderQueue>(files.Length);

            var charBuffer = new char[inputBufferSize * files.Length];

            var binaryHeap = new BinaryHeap<ChunkLineRecord>(files.Length, new ChunkLineRecordComparer());

            fixed (char* pCharBuffer = charBuffer)
            {
                for (var i = 0; i < files.Length; i++)
                {
                    var chunk = new ChunkReaderQueue(files[i], i, inputBufferSize, pCharBuffer + (inputBufferSize * i), inputBufferSize);
                    chunks.Add(i, chunk);

                    binaryHeap.Insert(chunk.Dequeue());
                }

                var comparer = new LineRecordComparer();

                using (var streamWriter = new StreamWriter(outputFile, false, Encoding.UTF8, outputBufferSize))
                {
                    while (chunks.Count > 1)
                    {
                        var minLineRecord = binaryHeap.RemoveRoot();
                        streamWriter.Write(minLineRecord.LineRecord.NumberPart);
                        streamWriter.Write(". ");
                        streamWriter.WriteLine(new Span<char>(&pCharBuffer[minLineRecord.ChunkIndex * inputBufferSize], minLineRecord.LineRecord.StringPartLength));

                        var minChunk = chunks[minLineRecord.ChunkIndex];
                        if (minChunk.IsEmpty)
                        {
                            progress?.Report($"Chunk {Path.GetFileNameWithoutExtension(minChunk.ChunkFile)} depleted! ({files.Length - chunks.Count + 1}/{files.Length})");
                            chunks.Remove(minLineRecord.ChunkIndex);
                            minChunk.Dispose();
                        }
                        else
                        {
                            binaryHeap.Insert(minChunk.Dequeue());
                        }
                    }

                    var lastChunk = chunks.First().Value;

                    while (!lastChunk.IsEmpty)
                    {
                        var lineRecord = lastChunk.Peek();

                        streamWriter.Write(lineRecord.LineRecord.NumberPart);
                        streamWriter.Write(". ");
                        streamWriter.WriteLine(new Span<char>(&pCharBuffer[lineRecord.ChunkIndex * inputBufferSize], lineRecord.LineRecord.StringPartLength));

                        lastChunk.Dequeue();
                    }

                    lastChunk.Dispose();
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
