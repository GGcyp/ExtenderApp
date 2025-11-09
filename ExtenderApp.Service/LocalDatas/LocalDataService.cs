using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        /// 存储本地数据信息的字典。
        /// </summary>
        private readonly Dictionary<string, LocalDataInfo> _localDataDict;

        /// <summary>
        /// 日志服务接口，用于记录日志信息。
        /// </summary>
        private readonly ILogger<ILocalDataService> _logger;

        /// <summary>
        /// 服务提供者实例，用于解析依赖服务。
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 默认数据版本号。 用于初始化本地数据的版本信息，未指定时采用此版本。
        /// </summary>
        private readonly Version _version;

        /// <summary>
        /// 自动保存任务的取消令牌。
        /// </summary>
        private ScheduledTask autosaveTokn;

        public LocalDataService(IBinaryParser parser, IBinaryFormatterStore store, ILogger<ILocalDataService> logger, IServiceProvider serviceProvider)
        {
            _parser = parser;

            _localDataDict = new();
            _logger = logger;
            _version = new(0, 0, 0, 1);

            autosaveTokn = new ScheduledTask();
            autosaveTokn.StartCycle(o => SaveAllData(), TimeSpan.FromMinutes(5));
            _serviceProvider = serviceProvider;
        }

        public bool LoadData<T>(string? dataName, out LocalData<T>? data)
            where T : class
        {
            data = default;

            if (string.IsNullOrEmpty(dataName))
            {
                _logger.LogError("获取本地数据名字不能为空");
                return false;
            }

            try
            {
                LocalData<T>? localData = null;
                if (!_localDataDict.TryGetValue(dataName, out var info))
                {
                    ExpectLocalFileInfo fileInfo = new(ProgramDirectory.DataPath, dataName);
                    var tempdata = _parser.Read<VersionData<T>>(fileInfo);

                    localData = new LocalData<T>(tempdata.Data ?? _serviceProvider.GetRequiredService<T>(), SaveLocalData, tempdata.DataVersion ?? _version);

                    info = new LocalDataInfo(ProgramDirectory.DataPath, dataName, localData);
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
                _logger.LogError(ex, "读取本地数据出现错误:{dataName}", dataName);

                if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                    return false;

                data = new LocalData<T>(Activator.CreateInstance(typeof(T)) as T, SaveLocalData, _version);
                var info = new LocalDataInfo(ProgramDirectory.DataPath, dataName, data);
                _localDataDict.Add(dataName, info);
                return true;
            }
        }

        public bool SaveData<T>(string? dataName, LocalData<T>? data)
            where T : class
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
                _logger.LogError(ex, "写入本地数据出现错误:{dataName}", dataName);
                return false;
            }
        }

        public bool DeleteData(string? dataName)
        {
            if (string.IsNullOrEmpty(dataName))
            {
                return false;
            }

            return _localDataDict.Remove(dataName);
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