namespace ExtenderApp.Data
{
    public class ExtenderStream : Stream
    {
        private readonly Stream _stream;

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public FileOperate FileOperate { get; private set; }

        public ExtenderStream(Stream stream, FileOperate operate)
        {
            _stream = stream;
            FileOperate = operate;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (FileOperate.FileAccess == FileAccess.Write)
                throw new InvalidOperationException("文件操作不支持读取操作");

            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (FileOperate.FileAccess == FileAccess.Read)
                throw new InvalidOperationException("文件操作不支持写入操作");

            _stream.Write(buffer, offset, count);
        }
    }
}
