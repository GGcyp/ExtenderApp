﻿using System;
using System.Reflection;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Services;


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
        /// 当前版本信息。
        /// </summary>
        private readonly Version _version;

        /// <summary>
        /// 日志服务接口，用于记录日志信息。
        /// </summary>
        private readonly ILogingService _logingService;

        /// <summary>
        /// 自动保存任务的取消令牌。
        /// </summary>
        private ScheduledTask autosaveTokn;

        /// <summary>
        /// 方法参数数组。
        /// </summary>
        private object?[] methodParameters;

        /// <summary>
        /// 序列化方法的信息。
        /// </summary>
        private MethodInfo writeMethod;

        public LocalDataService(IPathService pathService, ISplitterParser splitter, IBinaryParser parser, IBinaryFormatterStore store, ILogingService logingService)
        {
            _parser = parser;
            _pathService = pathService;
            _version = new Version("0.0.0.1");

            _localDataDict = new();
            _logingService = logingService;

            autosaveTokn = new ScheduledTask();
            autosaveTokn.StartCycle(o => SaveAllData(), TimeSpan.FromMinutes(5));

            //获取函数信息
            methodParameters = new object?[3];
            var methods = typeof(IFileParser).GetMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == "Write" && method.IsGenericMethodDefinition)
                {
                    //var parameters = method.GetParameters();
                    //if (parameters.Length == 3 &&
                    //    parameters[0].ParameterType == typeof(ExpectLocalFileInfo) &&
                    //    parameters[1].ParameterType.IsGenericParameter &&
                    //    parameters[2].ParameterType == typeof(object))
                    //{
                    //    return method.MakeGenericMethod(type);
                    //}
                    var parameters = method.GetParameters();
                    if (parameters[0].ParameterType == typeof(ExpectLocalFileInfo))
                    {
                        writeMethod = method;
                        break;
                    }
                }
            }
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

                LocalData<T>? localData = null;
                if (!_localDataDict.TryGetValue(dataName, out var info))
                {
                    info = new LocalDataInfo(_pathService.DataPath, dataName, CreateSerializeMethodInfo(typeof(LocalData<T>)));

                    _localDataDict.Add(dataName, info);

                    localData = _parser.Read<LocalData<T>>(info.FileInfo);

                    if (localData is null)
                    {
                        //如果读取失败则创建一个新的LocalData对象
                        localData = new LocalData<T>(Activator.CreateInstance(typeof(T)) as T, _version);
                    }
                    info.LocalData = localData;

                    //如果版本不一致则更新
                    //VersionCheck(info);
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

        public bool SaveData<T>(string? dataName, LocalData<T>? data) where T : class
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
                    info = new LocalDataInfo(_pathService.DataPath, dataName, CreateSerializeMethodInfo(typeof(LocalData<T>)));
                    _localDataDict.Add(dataName, info);
                }

                info.LocalData = localData = data;

                _parser.WriteAsync(info.FileInfo, localData, CompressionType.Lz4Block);
                return true;
            }
            catch (Exception ex)
            {
                _logingService.Error("写入本地数据出现错误", nameof(ILocalDataService), ex);
                return false;
            }
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

        /// <summary>
        /// 创建一个序列化方法的信息对象。
        /// </summary>
        /// <param name="type">需要序列化的数据类型。</param>
        /// <returns>返回创建的序列化方法信息对象。</returns>
        private MethodInfo CreateSerializeMethodInfo(Type type)
        {
            return writeMethod.MakeGenericMethod(type);
        }

        /// <summary>
        /// 保存所有数据。
        /// </summary>
        private void SaveAllData()
        {
            foreach (var data in _localDataDict.Values)
            {
                methodParameters[0] = data.FileInfo;
                methodParameters[1] = data.LocalData;
                var temp = data.SerializeMethodInfo?.Invoke(_parser, methodParameters);
            }
        }
    }
}
