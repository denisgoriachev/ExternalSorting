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

            //MergeSortedChunks(temporaryFolder, outputFile, availableRam, outputBufferSize, progress);
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
