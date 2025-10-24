namespace ExtenderApp.Data
{
    public ref struct LinkClientPluginSendData
    {
        public readonly ByteBuffer DataBuffer;
        public int DataType;
        public ByteBuffer FirstBuffer;
        public ByteBuffer OutDataBuffer;
        public ByteBuffer LastBuffer;

        public LinkClientPluginSendData(ByteBuffer dataBuffer, int dataType)
        {
            DataBuffer = dataBuffer;
            DataType = dataType;
            OutDataBuffer = dataBuffer;
        }

        public void Dispose()
        {
            DataBuffer.Dispose();
            FirstBuffer.Dispose();
            OutDataBuffer.Dispose();
            LastBuffer.Dispose();
        }

        public ByteBlock ToBlock()
        {
            int length = (int)(FirstBuffer.Remaining + OutDataBuffer.Remaining + LastBuffer.Remaining);
            ByteBlock byteBlock = new ByteBlock(length);

            if (FirstBuffer.Remaining > 0)
                byteBlock.Write(FirstBuffer);
            if (OutDataBuffer.Remaining > 0)
                byteBlock.Write(OutDataBuffer);
            if (LastBuffer.Remaining > 0)
                byteBlock.Write(LastBuffer);

            return byteBlock;
        }
    }
}