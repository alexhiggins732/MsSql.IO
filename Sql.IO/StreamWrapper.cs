using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sql.IO
{
    /// <summary>
    /// A utility class to wrap a prexisiting stream. Used to expose SqlFileStream to external libraries without requiring Custom Dll references.
    /// Otherwise, all websites and APIs using this libary would be required to install SqlClr dlls which additionally contains DataTypes
    /// not currently support on .Net Stander or .Net Core single SqlFileStream is currently only compatiable with MsSql on windows.
    /// </summary>
    public class StreamWrapper : Stream, IDisposable
    {
        //TODO: Document stream wrapper methods
        private Stream baseStream;
        protected bool modified = false;
        protected StreamWrapper() { }
        protected void setStream(Stream baseStream) => this.baseStream = baseStream;
        public StreamWrapper(Stream baseStream) => this.baseStream = baseStream;

        public override bool CanRead => baseStream.CanRead;

        public override bool CanSeek => baseStream.CanSeek;

        public override bool CanWrite => baseStream.CanWrite;

        public override long Length => baseStream.Length;

        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

        void IDisposable.Dispose() => baseStream.Dispose();

        public override void Flush() => baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => baseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);

        public override void SetLength(long value) => baseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.modified = true;
            baseStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.modified = true;
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.modified = true;
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            this.modified = true;
            base.WriteByte(value);
        }
    }
}
