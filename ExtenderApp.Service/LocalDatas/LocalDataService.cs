using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;


namespace ExtenderApp.Services
{
    /// <summary>
    /// 本地数据服务类，实现了ILocalDataService接口。
    /// </summary>
    internal class LocalDataService : ILocalDataService
    {
        /// <summary>
        /// 二进制解析器接口，用于解析二进制数据。
        /// </summary>
        private readonly IBinaryParser _parser;

        /// <summary>
        /// 路径服务接口，用于处理文件路径相关操作。
        /// </summary>
        private readonly IPathService _pathService;

        /// <summary>
        /// 存储本地数据信息的字典。
        /// </summary>
        private readonly Dictionary<string, LocalDataInfo> _localDataDict;

        /// <summary>
        /// 日志服务接口，用于记录日志信息。
        /// </summary>
        private readonly ILogingService _logingService;

        private readonly Version _version;

        /// <summary>
        /// 自动保存任务的取消令牌。
        /// </summary>
        private ScheduledTask autosaveTokn;

        public LocalDataService(IPathService pathService, ISplitterParser splitter, IBinaryParser parser, IBinaryFormatterStore store, ILogingService logingService)
        {
            _parser = parser;
            _pathService = pathService;

            _localDataDict = new();
            _logingService = logingService;
            _version = new(0, 0, 0, 1);

            autosaveTokn = new ScheduledTask();
            autosaveTokn.StartCycle(o => SaveAllData(), TimeSpan.FromMinutes(5));
        }

        public bool LoadData<T>(string? dataName, out LocalData<T>? data) where T : class
        {
            data = default;

            try
            {
                if (string.IsNullOrEmpty(dataName))
                {
                    throw new ArgumentNullException("获取本地数据名字不能为空");
                }
            }
            catch (Exception ex)
            {
                _logingService.Error(ex.Message, nameof(ILocalDataService), ex);
                return false;
            }

            try
            {
                LocalData<T>? localData = null;
                if (!_localDataDict.TryGetValue(dataName, out var info))
                {
                    ExpectLocalFileInfo fileInfo = new(_pathService.DataPath, dataName);
                    var tempdata = _parser.Read<VersionData<T>>(fileInfo);

                    localData = new LocalData<T>(tempdata.Data ?? Activator.CreateInstance(typeof(T)) as T, SaveLocalData, tempdata.DataVersion ?? _version);

                    info = new LocalDataInfo(_pathService.DataPath, dataName, localData);
                    _localDataDict.Add(dataName, info);
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
                _logingService.Error($"读取本地数据出现错误:{dataName}", nameof(ILocalDataService), ex);

                if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                    return false;

                data = new LocalData<T>(Activator.CreateInstance(typeof(T)) as T, SaveLocalData, _version);
                var info = new LocalDataInfo(_pathService.DataPath, dataName, data);
                _localDataDict.Add(dataName, info);
                return true;
            }
        }

        public bool SaveData<T>(string? dataName, LocalData<T>? data) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(dataName))
                {
                    throw new ArgumentNullException("保存本地数据名字不能为空");
                }
                if (data is null || data.Data is null || data.Version == null)
                {
                    throw new ArgumentNullException("保存本地数据不能为空");
                }

                if (!_localDataDict.TryGetValue(dataName, out var info))
                {
                    //info = new LocalDataInfo(_pathService.DataPath, dataName, data);
                    //_localDataDict.Add(dataName, info);
                    return false;
                }
                else
                {
                    if (info.LocalData != data)
                        info.LocalData = data;

                    if (data.SaveAcion is null)
                        data.SaveAcion = SaveLocalData;
                }

                _parser.WriteAsync(info.FileInfo, data.ToVersionData(), CompressionType.Lz4Block);
                return true;
            }
            catch (Exception ex)
            {
                _logingService.Error($"写入本地数据出现错误:{dataName}", nameof(ILocalDataService), ex);
                return false;
            }
        }

        private void SaveLocalData<T>(ExpectLocalFileInfo info, Version version, T data)
        {
            _parser.Write(info, new VersionData<T>(version, data), CompressionType.Lz4Block);
        }

        /// <summary>
        /// 保存所有数据。
        /// </summary>
        private void SaveAllData()
        {
            foreach (var data in _localDataDict.Values)
            {
                data.Save();
            }
        }
    }
}
