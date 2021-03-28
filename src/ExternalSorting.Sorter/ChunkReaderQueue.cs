using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    internal class ChunkReaderQueue : IDisposable
    {
        private bool disposedValue;
        private readonly FileStream _fileStream;
        private BinaryReader _binaryReader;
        private LineRecord _head;

        public int ChunkSize { get; }

        public int ReadedRecords { get; private set; }

        public bool IsEmpty => ReadedRecords == ChunkSize;

        public ChunkReaderQueue(string chunkFile, int readCapacity)
        {
            _fileStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read, FileShare.None, readCapacity);
            _binaryReader = new BinaryReader(_fileStream);

            ChunkSize = _binaryReader.ReadInt32();
            ReadedRecords = 0;

            ReadOne(_binaryReader, ref _head);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _binaryReader.Dispose();
                    _fileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                disposedValue = true;
            }
        }

        public LineRecord Peek()
        {
            if (IsEmpty)
            {
                throw new Exception("Chunk Queue is empty.");
            }

            return _head;
        }


        public bool Dequeue()
        {
            if (IsEmpty)
            {
                return false;
            }

            ReadOne(_binaryReader, ref _head);
            ReadedRecords++;

            return true;
        }

        static bool ReadOne(BinaryReader binaryReader, ref LineRecord record)
        {
            if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
                return false;

            record.NumberPart = binaryReader.ReadInt32();
            var stringLength = binaryReader.ReadInt32();
            record.StringPart = new string(binaryReader.ReadChars(stringLength));

            return true;

        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ChunkReader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
