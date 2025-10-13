using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    public class FileNodeParentFormatter<TParent, TFileNode> : ResolverFormatter<TParent>
        where TParent : FileNodeParent<TFileNode>, new()
        where TFileNode : FileNode<TFileNode>, new()
    {
        protected readonly IBinaryFormatter<TFileNode> _fileNode;
        protected readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _fileNode.DefaultLength + _string.DefaultLength;

        public FileNodeParentFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _fileNode = GetFormatter<TFileNode>();
            _string = GetFormatter<string>();
        }

        public override TParent Deserialize(ref ByteBuffer buffer)
        {
            TParent nodeParent = new TParent();
            nodeParent.ParentPath = _string.Deserialize(ref buffer);
            nodeParent.ParentNode = _fileNode.Deserialize(ref buffer);
            return nodeParent;
        }

        public override void Serialize(ref ByteBuffer buffer, TParent value)
        {
            _string.Serialize(ref buffer, value.ParentPath);
            _fileNode.Serialize(ref buffer, value.ParentNode);
        }

        public override long GetLength(TParent value)
        {
            return _string.GetLength(value.ParentPath) + _fileNode.GetLength(value.ParentNode);
        }
    }
}
