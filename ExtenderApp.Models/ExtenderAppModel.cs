﻿using ExtenderApp.Abstract;

namespace ExtenderApp.Models
{
    /// <summary>
    /// Model层，Model基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ExtenderAppModel<TDto> : IModel<TDto> where TDto : class,IDto,new()
    {
        public IModelConverterExecutor Converter { get; }

        public ExtenderAppModel(IModelConverterExecutor converter)
        {
            Converter = converter;
        }

        #region 基础操作

        public abstract void AddDataSource(object? data);

        public abstract object? GetDataSource();

        public abstract void Add(TDto dto);

        public abstract void Clear();

        public abstract void Remove(TDto dto);

        public abstract TDto? Get(object key);

        public abstract void ForEach(Action<TDto> action);

        #endregion
    }
}
