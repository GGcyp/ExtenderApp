using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtenderApp.Torrent.Models;

namespace TorrentUtility
{
    public class TorrentDownloader
    {
        private MagnetLink _magnetLink;
        private string _downloadPath;
        private CancellationTokenSource _cancellationTokenSource;
        private Dictionary<string, PeerConnection> _peers;
        private bool _isDownloading;

        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
        public event EventHandler<DownloadErrorEventArgs> DownloadError;

        public TorrentDownloader(MagnetLink magnetLink, string downloadPath)
        {
            _magnetLink = magnetLink ?? throw new ArgumentNullException(nameof(magnetLink));
            _downloadPath = downloadPath ?? throw new ArgumentNullException(nameof(downloadPath));

            if (!Directory.Exists(_downloadPath))
                Directory.CreateDirectory(_downloadPath);

            _peers = new Dictionary<string, PeerConnection>();
            _isDownloading = false;
        }

        public async Task StartDownloadAsync()
        {
            if (_isDownloading)
                return;

            _isDownloading = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // 1. 获取torrent文件
                string torrentFilePath = await FetchTorrentFileAsync(_cancellationTokenSource.Token);
                if (string.IsNullOrEmpty(torrentFilePath))
                    throw new Exception("无法获取torrent文件");

                // 2. 解析torrent文件
                TorrentFile torrentFile = ParseTorrentFile(torrentFilePath);

                // 3. 连接DHT网络获取更多种子
                await ConnectToDhtNetworkAsync(torrentFile, _cancellationTokenSource.Token);

                // 4. 连接tracker服务器获取对等节点
                await ConnectToTrackersAsync(torrentFile, _cancellationTokenSource.Token);

                // 5. 开始下载
                await StartPeerDownloadsAsync(torrentFile, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                OnDownloadError("下载已取消");
            }
            catch (Exception ex)
            {
                OnDownloadError($"下载过程中发生错误: {ex.Message}");
            }
            finally
            {
                _isDownloading = false;
                CleanupResources();
            }
        }

        public void StopDownload()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task<string> FetchTorrentFileAsync(CancellationToken cancellationToken)
        {
            // 从磁链中获取torrent文件
            // 这里可能需要连接DHT网络或使用torrent文件服务器

            // 示例：如果磁链中有可接受的来源，可以尝试从那里获取
            if (!string.IsNullOrEmpty(_magnetLink.AcceptableSource))
            {
                try
                {
                    string torrentFilePath = Path.Combine(_downloadPath, $"{_magnetLink.Name}.torrent");
                    using (WebClient client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(new Uri(_magnetLink.AcceptableSource), torrentFilePath);
                        return torrentFilePath;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"从可接受来源获取torrent文件失败: {ex.Message}");
                }
            }

            // 如果没有可接受的来源或获取失败，可以尝试通过DHT网络获取
            // 这里简化处理，返回一个临时文件路径
            return Path.Combine(_downloadPath, "temp.torrent");
        }

        private TorrentFile ParseTorrentFile(string torrentFilePath)
        {
            // 解析torrent文件内容
            // 实际实现需要处理B编码格式
            return new TorrentFile
            {
                InfoHash = _magnetLink.Hashes.Values.FirstOrDefault(),
                PieceLength = long.Parse(_magnetLink.PieceLength ?? "0"),
                Pieces = new List<string>(), // 从torrent文件中解析
                Files = new List<TorrentFileInfo>
                {
                    new TorrentFileInfo
                    {
                        Path = _magnetLink.Name,
                        Length = long.Parse(_magnetLink.ContentLength ?? "0")
                    }
                }
            };
        }

        private async Task ConnectToDhtNetworkAsync(TorrentFile torrentFile, CancellationToken cancellationToken)
        {
            // 连接到DHT网络以发现更多种子和对等节点
            // 简化实现，实际需要实现Kademlia DHT协议

            // 这里只是模拟添加一些节点
            await Task.Delay(1000, cancellationToken);

            // 添加DHT发现的对等节点
            AddPeers(new List<string>
            {
                "192.168.1.1:6881",
                "192.168.1.2:6881",
                "192.168.1.3:6881"
            });
        }

        private async Task ConnectToTrackersAsync(TorrentFile torrentFile, CancellationToken cancellationToken)
        {
            // 连接到所有tracker服务器获取对等节点列表
            var tasks = new List<Task>();

            foreach (string trackerUrl in _magnetLink.Trackers)
            {
                tasks.Add(ConnectToTrackerAsync(torrentFile, trackerUrl, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ConnectToTrackerAsync(TorrentFile torrentFile, string trackerUrl, CancellationToken cancellationToken)
        {
            try
            {
                // 实现与tracker服务器的通信
                // 发送HTTP/HTTPS请求或UDP包到tracker服务器
                // 解析返回的对等节点列表

                // 简化示例，假设我们从tracker获取了一些对等节点
                await Task.Delay(500, cancellationToken);

                // 添加从tracker获取的对等节点
                AddPeers(new List<string>
                {
                    "192.168.1.10:6881",
                    "192.168.1.11:6881",
                    "192.168.1.12:6881"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接到tracker {trackerUrl} 失败: {ex.Message}");
            }
        }

        private async Task StartPeerDownloadsAsync(TorrentFile torrentFile, CancellationToken cancellationToken)
        {
            // 连接到所有对等节点并开始下载
            var downloadTasks = new List<Task>();

            foreach (var peerConnection in _peers.Values)
            {
                downloadTasks.Add(DownloadFromPeerAsync(peerConnection, torrentFile, cancellationToken));
            }

            // 等待所有下载任务完成或取消
            try
            {
                await Task.WhenAll(downloadTasks);
                OnDownloadCompleted(true);
            }
            catch (OperationCanceledException)
            {
                OnDownloadCompleted(false);
            }
            catch (Exception ex)
            {
                OnDownloadError($"下载过程中发生错误: {ex.Message}");
                OnDownloadCompleted(false);
            }
        }

        private async Task DownloadFromPeerAsync(PeerConnection peer, TorrentFile torrentFile, CancellationToken cancellationToken)
        {
            try
            {
                // 连接到对等节点
                await peer.ConnectAsync(cancellationToken);

                // 发送握手消息
                await peer.SendHandshakeAsync(torrentFile.InfoHash, cancellationToken);

                // 接收对等节点状态
                await peer.ReceivePeerStatusAsync(cancellationToken);

                // 开始下载文件片段
                await peer.DownloadPiecesAsync(torrentFile, _downloadPath,
                    (progress) => OnDownloadProgress(peer.PeerId, progress),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从对等节点 {peer.EndPoint} 下载失败: {ex.Message}");
                peer.Disconnect();
            }
        }

        private void AddPeers(List<string> peerEndPoints)
        {
            foreach (string endPoint in peerEndPoints)
            {
                if (!_peers.ContainsKey(endPoint))
                {
                    _peers[endPoint] = new PeerConnection(endPoint);
                }
            }
        }

        private void CleanupResources()
        {
            // 清理资源，关闭所有对等连接
            foreach (var peer in _peers.Values)
            {
                peer.Disconnect();
            }
            _peers.Clear();
        }

        protected virtual void OnDownloadProgress(string peerId, double progress)
        {
            DownloadProgress?.Invoke(this, new DownloadProgressEventArgs(peerId, progress));
        }

        protected virtual void OnDownloadCompleted(bool success)
        {
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(success));
        }

        protected virtual void OnDownloadError(string errorMessage)
        {
            DownloadError?.Invoke(this, new DownloadErrorEventArgs(errorMessage));
        }
    }

    public class TorrentFile
    {
        public string InfoHash { get; set; }
        public long PieceLength { get; set; }
        public List<string> Pieces { get; set; }
        public List<TorrentFileInfo> Files { get; set; }
    }

    public class TorrentFileInfo
    {
        public string Path { get; set; }
        public long Length { get; set; }
    }

    public class PeerConnection
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;

        public string PeerId { get; private set; }
        public string EndPoint { get; private set; }

        public PeerConnection(string endPoint)
        {
            EndPoint = endPoint;
            _client = new TcpClient();
            _isConnected = false;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                string[] parts = EndPoint.Split(':');
                string host = parts[0];
                int port = int.Parse(parts[1]);

                await _client.ConnectAsync(host, port, cancellationToken);
                _stream = _client.GetStream();
                _isConnected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接到对等节点 {EndPoint} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task SendHandshakeAsync(string infoHash, string clientId, CancellationToken cancellationToken)
        {
            if (!_isConnected)
                throw new InvalidOperationException("未连接到对等节点");

            try
            {
                // 构建握手消息
                byte[] handshake = BuildHandshake(infoHash, clientId);

                // 发送握手消息
                await _stream.WriteAsync(handshake, 0, handshake.Length, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送握手消息到 {EndPoint} 失败: {ex.Message}");
                throw;
            }
        }

        private byte[] BuildHandshake(string infoHash, string clientId)
        {
            // 握手消息格式: <pstrlen><pstr><reserved><info_hash><peer_id>
            // 对于BitTorrent协议，pstrlen=19, pstr="BitTorrent protocol"
            byte[] handshake = new byte[68];

            // pstrlen
            handshake[0] = 19;

            // pstr
            byte[] pstr = Encoding.ASCII.GetBytes("BitTorrent protocol");
            Array.Copy(pstr, 0, handshake, 1, pstr.Length);

            // reserved (8 bytes of zeros)
            // info_hash (20 bytes)
            byte[] infoHashBytes = Encoding.ASCII.GetBytes(infoHash);
            Array.Copy(infoHashBytes, 0, handshake, 28, Math.Min(infoHashBytes.Length, 20));

            // peer_id (20 bytes)
            byte[] peerIdBytes = Encoding.ASCII.GetBytes(clientId);
            Array.Copy(peerIdBytes, 0, handshake, 48, Math.Min(peerIdBytes.Length, 20));

            return handshake;
        }

        public async Task ReceivePeerStatusAsync(CancellationToken cancellationToken)
        {
            // 接收并解析对等节点的状态消息
            // 简化实现
            await Task.Delay(100, cancellationToken);
        }

        public async Task DownloadPiecesAsync(TorrentFile torrentFile, string downloadPath,
            Action<double> progressCallback, CancellationToken cancellationToken)
        {
            // 从对等节点下载文件片段
            // 简化实现
            for (int i = 0; i < 100; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(100, cancellationToken);
                progressCallback?.Invoke(i / 100.0);
            }
        }

        public void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                _isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭对等连接 {EndPoint} 失败: {ex.Message}");
            }
        }
    }

    public class DownloadProgressEventArgs : EventArgs
    {
        public string PeerId { get; }
        public double Progress { get; }

        public DownloadProgressEventArgs(string peerId, double progress)
        {
            PeerId = peerId;
            Progress = progress;
        }
    }

    public class DownloadCompletedEventArgs : EventArgs
    {
        public bool Success { get; }

        public DownloadCompletedEventArgs(bool success)
        {
            Success = success;
        }
    }

    public class DownloadErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }

        public DownloadErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
