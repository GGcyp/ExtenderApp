using System.IO;
using System.Security.Cryptography;
using ExtenderApp.Common;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// BitTorrent 文件管理器
    /// </summary>
    public class TorrentFileManager : DisposableObject
    {
        private readonly string _downloadPath;
        private readonly long _fileSize;
        private readonly int _pieceLength;
        private readonly byte[][] _pieceHashes;
        private readonly FileStream _fileStream;
        private readonly bool _isMultiFile;
        private readonly List<TorrentFile> _files;
        private readonly BitFieldData _bitField;
        private readonly SHA1 _sha1 = SHA1.Create();

        public string DownloadPath => _downloadPath;
        public long FileSize => _fileSize;
        public int PieceLength => _pieceLength;
        public int PieceCount => _pieceHashes.Length;
        public BitFieldData BitField => _bitField;

        public TorrentFileManager(string downloadPath, long fileSize, int pieceLength,
            byte[][] pieceHashes, bool isMultiFile = false, List<TorrentFile> files = null)
        {
            _downloadPath = downloadPath;
            _fileSize = fileSize;
            _pieceLength = pieceLength;
            _pieceHashes = pieceHashes;
            _isMultiFile = isMultiFile;
            _files = files ?? new List<TorrentFile>();
            _bitField = new BitFieldData(pieceHashes.Length);

            // 创建文件或目录
            if (_isMultiFile)
            {
                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);
            }
            else
            {
                var directory = Path.GetDirectoryName(downloadPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                _fileStream = new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
        }

        public async Task WriteBlockAsync(int pieceIndex, int begin, byte[] block)
        {
            if (_isMultiFile)
            {
                // 多文件模式：计算块在各文件中的位置
                long globalOffset = (long)pieceIndex * _pieceLength + begin;
                await WriteToMultiFilesAsync(globalOffset, block);
            }
            else
            {
                // 单文件模式：直接写入
                long offset = (long)pieceIndex * _pieceLength + begin;
                _fileStream.Seek(offset, SeekOrigin.Begin);
                await _fileStream.WriteAsync(block, 0, block.Length);
            }

            // 检查分片是否完整并验证哈希
            if (IsPieceComplete(pieceIndex))
            {
                if (await VerifyPieceAsync(pieceIndex))
                {
                    _bitField[pieceIndex] = true;
                    Console.WriteLine($"分片 {pieceIndex} 验证成功");
                }
                else
                {
                    Console.WriteLine($"分片 {pieceIndex} 验证失败，将重新下载");
                    // 分片验证失败，可选择删除已下载部分
                }
            }
        }

        private async Task<bool> VerifyPieceAsync(int pieceIndex)
        {
            byte[] pieceData = new byte[GetPieceSize(pieceIndex)];
            long offset = (long)pieceIndex * _pieceLength;

            if (_isMultiFile)
            {
                // 从多个文件中读取分片数据
                await ReadFromMultiFilesAsync(offset, pieceData);
            }
            else
            {
                // 从单个文件中读取分片数据
                _fileStream.Seek(offset, SeekOrigin.Begin);
                await _fileStream.ReadAsync(pieceData, 0, pieceData.Length);
            }

            // 计算SHA-1哈希
            byte[] computedHash = _sha1.ComputeHash(pieceData);
            return computedHash.SequenceEqual(_pieceHashes[pieceIndex]);
        }

        private int GetPieceSize(int pieceIndex)
        {
            long lastPieceSize = _fileSize % _pieceLength;
            return pieceIndex == PieceCount - 1 ? (int)lastPieceSize : _pieceLength;
        }

        private async Task WriteToMultiFilesAsync(long globalOffset, byte[] data)
        {
            long remaining = data.Length;
            long dataOffset = 0;

            foreach (var file in _files)
            {
                if (globalOffset >= file.Length)
                {
                    globalOffset -= file.Length;
                    continue;
                }

                long writeLength = Math.Min(remaining, file.Length - globalOffset);
                if (writeLength <= 0)
                    break;

                using (var stream = new FileStream(
                    Path.Combine(_downloadPath, file.Path),
                    FileMode.OpenOrCreate,
                    FileAccess.Write))
                {
                    stream.Seek(globalOffset, SeekOrigin.Begin);
                    await stream.WriteAsync(data, (int)dataOffset, (int)writeLength);
                }

                remaining -= writeLength;
                dataOffset += writeLength;
                globalOffset = 0;

                if (remaining <= 0)
                    break;
            }
        }

        private async Task ReadFromMultiFilesAsync(long globalOffset, byte[] buffer)
        {
            long remaining = buffer.Length;
            long bufferOffset = 0;

            foreach (var file in _files)
            {
                if (globalOffset >= file.Length)
                {
                    globalOffset -= file.Length;
                    continue;
                }

                long readLength = Math.Min(remaining, file.Length - globalOffset);
                if (readLength <= 0)
                    break;

                using (var stream = new FileStream(
                    Path.Combine(_downloadPath, file.Path),
                    FileMode.Open,
                    FileAccess.Read))
                {
                    stream.Seek(globalOffset, SeekOrigin.Begin);
                    await stream.ReadAsync(buffer, (int)bufferOffset, (int)readLength);
                }

                remaining -= readLength;
                bufferOffset += readLength;
                globalOffset = 0;

                if (remaining <= 0)
                    break;
            }
        }

        private bool IsPieceComplete(int pieceIndex)
        {
            // 实际实现中需要跟踪每个分片的下载进度
            // 简化版：假设只要调用了WriteBlockAsync就认为数据完整
            return true;
        }

        public byte[] GetPieceData(int pieceIndex)
        {
            int pieceSize = GetPieceSize(pieceIndex);
            byte[] pieceData = new byte[pieceSize];
            long offset = (long)pieceIndex * _pieceLength;

            if (_isMultiFile)
            {
                // 从多个文件中读取
                using (var memoryStream = new MemoryStream(pieceData))
                {
                    ReadFromMultiFilesAsync(offset, pieceData).Wait();
                }
            }
            else
            {
                // 从单个文件中读取
                _fileStream.Seek(offset, SeekOrigin.Begin);
                _fileStream.Read(pieceData, 0, pieceSize);
            }

            return pieceData;
        }

        public void Dispose()
        {
            //_fileStream?.Dispose();
            //_sha1?.Dispose();
        }
    }
}
