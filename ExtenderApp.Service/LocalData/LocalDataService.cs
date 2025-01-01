using System.Text.Json.Serialization;
using System.Text.Json;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Text.Encodings.Web;
using ExtenderApp.Common;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExtenderApp.Service
{
    internal class LocalDataService : ILocalDataService
    {
        private class Temp : LocalData
        {
            public string Name { get; set; }
        }
        private readonly IJsonParser _parser;
        private readonly IPathService _pathService;
        private ValueList<LocalDataInfo> _localDataInfos;

        public LocalDataService(IPathService pathService, IJsonParser parser)
        {
            _parser = parser;
            _pathService = pathService;

            _localDataInfos = new();

            CheckLocalData();

            SetData("5555", new Temp() { Name = "sssss" });
            var temp = GetData("5555");
            byte[] objectBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new Temp() { Name = "sssss" });
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

            var result = _parser.Deserialize<LocalData>(info.LocalFileInfo);
            info.LocalData = result;
            return result;
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

            _parser.Serialize(info.LocalData, info.LocalFileInfo);
        }
    }
}
