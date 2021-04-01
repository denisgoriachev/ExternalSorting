using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    internal sealed unsafe class ChunkReaderQueue : IDisposable
    {
        private bool disposedValue;
        private readonly FileStream _fileStream;
        public readonly int ChunkIndex;
        private readonly char* _stringBuffer;
        private readonly int _stringBufferLength;
        private BinaryReader _binaryReader;
        private ChunkLineRecord _head;

        public readonly int ChunkSize;

        public int ReadedRecords { get; private set; }

        public bool IsEmpty => ReadedRecords == ChunkSize;

        public string ChunkFile { get; }

        public ChunkReaderQueue(string chunkFile, int chunkIndex, int fileBufferSize, char* stringBuffer, int stringBufferLength)
        {
            ChunkFile = chunkFile;
            ChunkIndex = chunkIndex;
            _stringBuffer = stringBuffer;
            _stringBufferLength = stringBufferLength;

            _fileStream = new FileStream(ChunkFile, FileMode.Open, FileAccess.Read, FileShare.None, fileBufferSize);
            _binaryReader = new BinaryReader(_fileStream, Encoding.UTF8);

            ChunkSize = _binaryReader.ReadInt32();
            ReadedRecords = 0;

            ReadOne(out _head);
        }

        private void Dispose(bool disposing)
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

        public ChunkLineRecord Peek()
        {
            if (IsEmpty)
            {
                throw new Exception("Chunk Queue is empty.");
            }

            return _head;
        }


        public ChunkLineRecord Dequeue()
        {
            var result = _head;

            ReadOne(out _head);
            ReadedRecords++;

            return result;
        }

        bool ReadOne(out ChunkLineRecord record)
        {
            record = new ChunkLineRecord();

            if (_binaryReader.BaseStream.Position == _binaryReader.BaseStream.Length)
            {

                return false;
            }

            record.ChunkIndex = ChunkIndex;

            record.LineRecord.NumberPart = _binaryReader.ReadInt32();
            var stringLength = _binaryReader.ReadInt32();

            record.LineRecord.StringPartLength = stringLength;

            var span = new Span<char>(_stringBuffer, stringLength);
            _binaryReader.Read(span);
            record.LineRecord.StringPart = _stringBuffer;

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
