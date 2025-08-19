using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
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

        public override TParent Deserialize(ref ExtenderBinaryReader reader)
        {
            TParent nodeParent = new TParent();
            nodeParent.ParentPath = _string.Deserialize(ref reader);
            nodeParent.ParentNode = _fileNode.Deserialize(ref reader);
            return nodeParent;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, TParent value)
        {
            _string.Serialize(ref writer, value.ParentPath);
            _fileNode.Serialize(ref writer, value.ParentNode);
        }

        public override long GetLength(TParent value)
        {
            return _string.GetLength(value.ParentPath) + _fileNode.GetLength(value.ParentNode);
        }
    }
}
