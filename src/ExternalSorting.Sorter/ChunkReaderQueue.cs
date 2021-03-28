using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Sorter
{
    internal class ChunkReaderQueue : IDisposable
    {
        private bool disposedValue;
        private readonly int _bufferCapactity;
        private BinaryReader _binaryReader;

        public int ChunkSize { get; }

        public int ReadedRecords { get; private set; }

        public ChunkReaderQueue(Stream stream, int bufferCapactity)
        {
            _bufferCapactity = bufferCapactity;

            _binaryReader = new BinaryReader(stream, Encoding.UTF8);
            ChunkSize = _binaryReader.ReadInt32();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
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
