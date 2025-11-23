using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks.LinkClients;

namespace ExtenderApp.Common.Networks
{
    public struct FileTransferDecision
    {
        private readonly IFileLinkClient _linkClient;
        public Guid FileId { get; }
        public string FileName { get; }
        public long FileSize { get; }

        internal FileTransferDecision(IFileLinkClient linkClient, PushFileRequest request)
        {
            _linkClient = linkClient;
            FileId = request.FileId;
            FileName = request.FileName;
            FileSize = request.FileSize;
        }

        public void OK()
        {
            //var response = new FileTransferResponse(FileId, true);
            //_linkClient.Send(response);
        }
    }
}
