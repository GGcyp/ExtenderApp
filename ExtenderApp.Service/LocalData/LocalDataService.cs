using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;


namespace ExtenderApp.Service
{
    internal class LocalDataService : ILocalDataService
    {
        private readonly IBinaryParser _parser;
        private readonly IPathService _pathService;
        private ValueList<LocalDataInfo> _localDataInfos;

        public LocalDataService(IPathService pathService, IBinaryParser parser, BinaryFormatterResolverStore store)
        {
            _parser = parser;
            _pathService = pathService;

            _localDataInfos = new();

            CheckLocalData();
            parser.Serialize(new(new LocalFileInfo("E:\\浏览器下载\\temo.ext"), FileMode.OpenOrCreate, FileAccess.ReadWrite), 10);
        }

        private void CheckLocalData()
        {
            string dataPath = _pathService.DataPath;

            var dataPaths = Directory.GetFiles(dataPath);

            foreach (var path in dataPaths)
            {
                _localDataInfos.Add(new LocalDataInfo(path));
            }
        }

        public LocalData? GetData(string dataName)
        {
            var info = _localDataInfos.Find((d, name) => d.LocalFileInfo.FileNameWithoutExtension == name, dataName);
            if (info is null) return null;

            //var result = _parser.Deserialize<LocalData>(info.LocalFileInfo);
            //info.LocalData = result;
            //return result;
            return null;
        }

        public void SetData(string dataName, LocalData data)
        {
            var info = _localDataInfos.Find((d, name) => d.LocalFileInfo.FileNameWithoutExtension == name, dataName);
            if (info is null)
            {
                info = new LocalDataInfo(Path.Combine(_pathService.DataPath, string.Concat(dataName, _pathService.JsonFileExtension)));
                _localDataInfos.Add(info);
            }

            if (data is not null)
                info.LocalData = data;

            //_parser.Serialize();
        }
    }
}
