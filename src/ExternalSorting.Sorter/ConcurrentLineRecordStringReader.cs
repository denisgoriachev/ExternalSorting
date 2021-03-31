namespace ExternalSorting.Sorter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ConcurrentLineRecordStringReader : IDisposable
    {
        private bool _disposedValue;
        private readonly StreamReader _streamReader;
        private static readonly object _lock = new object();
        private readonly char[] _remainder;
        private int _remainderLength;

        public ConcurrentLineRecordStringReader(string filename, Encoding encoding, bool detectEncodingFromByteOrderMask, int readBufferSize)
        {
            _streamReader = new StreamReader(filename, encoding, detectEncodingFromByteOrderMask, readBufferSize);
            _remainder = new char[readBufferSize];
            _remainderLength = 0;
        }

        public Task<int> ReadLineRecordsAsync(char[] buffer)
        {
            return Task.Run(() =>
            {
                lock (_lock)
                {
                    if (_remainderLength != 0)
                    {
                        Array.Copy(_remainder, 0, buffer, 0, _remainderLength);
                    }

                    var readedCharacters = _streamReader.Read(buffer, _remainderLength, buffer.Length - _remainderLength);

                    if (readedCharacters <= 0)
                        return 0;

                    readedCharacters += _remainderLength;
                    _remainderLength = 0; ;

                    var span = new Span<char>(buffer, 0, readedCharacters + _remainderLength);

                    var lastNewLine = span.LastIndexOf(Environment.NewLine);

                    if (lastNewLine != readedCharacters - Environment.NewLine.Length)
                    {
                        readedCharacters -= (readedCharacters - lastNewLine);

                        var reminderSpan = span.Slice(readedCharacters);
                        reminderSpan.CopyTo(_remainder);
                        _remainderLength = reminderSpan.Length;
                    }

                    return readedCharacters;
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _streamReader.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConcurrentLineRecordReader()
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
