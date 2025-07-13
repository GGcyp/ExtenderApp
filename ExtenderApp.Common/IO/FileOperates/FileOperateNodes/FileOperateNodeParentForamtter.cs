using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.FileOperates.FileOperateNodes
{
    public class FileOperateNodeParentForamtter<TParent, TFileNode> : FileNodeParentFormatter<TParent, TFileNode>
        where TParent : FileOperateNodeParent<TFileNode>, new()
        where TFileNode : FIleOperateNode<TFileNode>, new()
    {
        public FileOperateNodeParentForamtter(IBinaryFormatterResolver resolver) : base(resolver)
        {
        }
    }
}
