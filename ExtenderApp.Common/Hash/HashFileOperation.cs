using System.Security.Cryptography;
using ExtenderApp.Common.IO;

namespace ExtenderApp.Common.Hash
{
    internal class HashFileOperation : ConcurrentOperation<FileOperateData>
    {
        private HashAlgorithm hashAlgorithm;

        public byte[] HashBytes { get; private set; }

        public void Set(HashAlgorithm hashAlgorithm)
        {
            this.hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
        }

        public override void Execute(FileOperateData item)
        {
            HashBytes = hashAlgorithm.ComputeHash(item.FStream);
        }

        public override bool TryReset()
        {
            hashAlgorithm = null;
            HashBytes = null;
            return true;
        }
    }
}
