using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Services;


namespace ExtenderApp.Service
{
    internal class LocalDataService : ILocalDataService
    {
        private readonly IBinaryParser _parser;
        private readonly IPathService _pathService;
        private Dictionary<string, LocalDataInfo> _localDataDict;
        private readonly Version _version;
        private readonly ILogingService _logingService;

        public LocalDataService(IPathService pathService, IBinaryParser parser, IBinaryFormatterStore store, ILogingService logingService)
        {
            _parser = parser;
            _pathService = pathService;
            _version = new Version("0.0.0.1");

            _localDataDict = new();
            _logingService = logingService;
        }

        public bool GetData<T>(string? dataName, out LocalData<T>? data)
        {
            data = default;
            try
            {
                if (string.IsNullOrEmpty(dataName))
                {
                    throw new ArgumentNullException("获取本地数据名字不能为空");
                }

                LocalData<T>? localData = null;
                if (!_localDataDict.TryGetValue(dataName, out var info))
                {
                    info = new LocalDataInfo(Path.Combine(_pathService.DataPath, string.Concat(dataName, ".ext")));

                    if (!info.LocalFileInfo.Exists)
                    {
                        return false;
                    }

                    _localDataDict.Add(dataName, info);

                    info.LocalData = localData = _parser.Deserialize<LocalData<T>>(info.LocalFileInfo);
                    //如果版本不一致则更新
                    VersionCheck(info);
                }
                else
                {
                    localData = info.LocalData as LocalData<T>;
                }

                data = localData;
                return localData is not null;
            }
            catch (Exception ex)
            {
                _logingService.Error("读取本地数据出现错误", nameof(ILocalDataService), ex);
                return false;
            }
        }

        public bool SetData<T>(string? dataName, LocalData<T>? data)
        {
            try
            {
                if (string.IsNullOrEmpty(dataName))
                {
                    throw new ArgumentNullException("保存本地数据名字不能为空");
                }
                LocalData<T>? localData = null;
                if (!_localDataDict.TryGetValue(dataName, out var info))
                {
                    info = new LocalDataInfo(Path.Combine(_pathService.DataPath, string.Concat(dataName, ".ext")));
                    info.LocalData = localData = data;
                    _localDataDict.Add(dataName, info);
                }
                else
                {
                    localData = info.LocalData as LocalData<T>;
                }

                _parser.Serialize(info.LocalFileInfo, localData);
                return true;
            }
            catch (Exception ex)
            {
                _logingService.Error("写入本地数据出现错误", nameof(ILocalDataService), ex);
                return false;
            }
        }

        /// <summary>
        /// 创建一个LocalData对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">要创建的数据</param>
        /// <returns>创建的LocalData对象</returns>
        private LocalData<T> CreateLocalData<T>(T data, Version version)
        {
            return new LocalData<T>(data, version);
        }

        /// <summary>
        /// 版本检查
        /// </summary>
        /// <param name="info">LocalDataInfo对象</param>
        private void VersionCheck(LocalDataInfo info)
        {
            if (info.LocalData is null)
            {
                return;
            }
            if (info.LocalData.Version is null)
            {
                return;
            }
            if (info.LocalData.Version < _version)
            {
                //更新数据
            }
        }
    }
}
