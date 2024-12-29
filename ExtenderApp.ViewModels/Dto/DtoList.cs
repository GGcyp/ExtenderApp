using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using ExtenderApp.Data;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.ViewModels
{
    ///// <summary>
    ///// DtoList 泛型类，用于管理 DTO 对象列表，并与实体对象列表进行关联。
    ///// 该类实现了 ObjectPoolable 以支持对象池，INotifyPropertyChanged 和 INotifyCollectionChanged 以支持属性更改和集合更改通知，以及 IList<TDto> 以支持列表操作。
    ///// </summary>
    ///// <typeparam name="TDto">DTO 类型，必须继承自 BaseDto<TEntity> 并具有无参构造函数。</typeparam>
    ///// <typeparam name="TEntity">实体类型，必须为类类型并具有无参构造函数。</typeparam>
    //public class DtoList<TDto, TEntity>
    //    : ObjectPoolable<DtoList<TDto, TEntity>>,
    //        INotifyPropertyChanged,
    //        INotifyCollectionChanged,
    //        IList<TDto>
    //    where TDto : BaseDto<TEntity>, new()
    //    where TEntity : class, new()
    //{
    //    /// <summary>
    //    /// 静态对象池，用于存储和重用<typeparamref name="TDto"/>类型的对象。
    //    /// 使用对象池可以减少内存分配和垃圾回收的压力，提高性能。
    //    /// </summary>
    //    private static ObjectPool<TDto> pool { get; } = ObjectPool.Create<TDto>();

    //    /// <summary>
    //    /// DTO列表，包含与实体列表关联的DTO对象列表
    //    /// </summary>
    //    private ValueList<KeyValuePair<List<TEntity>?, TDto>> dtoList;

    //    /// <summary>
    //    /// 实体列表
    //    /// </summary>
    //    private ValueList<List<TEntity>> entityList;

    //    /// <summary>
    //    /// 当前DTO的序号
    //    /// </summary>
    //    public int DtoIndex { get; private set; }

    //    /// <summary>
    //    /// 每页可容纳的数量
    //    /// </summary>
    //    public int PageSize { get; private set; }

    //    /// <summary>
    //    /// 获取或设置筛选条件。
    //    /// 这是一个用于过滤实体集合的委托，其参数是实体类型 TEntity，返回值是布尔类型，表示是否满足筛选条件。
    //    /// </summary>
    //    public Predicate<TEntity>? Filter { get; set; }

    //    public DtoList(List<TEntity> entities)
    //        : this()
    //    {
    //        entityList.Add(entities);
    //        dtoList = new();
    //    }

    //    public DtoList()
    //        : this(10) { }

    //    public DtoList(int pageSize)
    //    {
    //        PageSize = pageSize;
    //        DtoIndex = 0;
    //    }

    //    #region OnChanged

    //    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    //    public void OnCollectionChanged(NotifyCollectionChangedEventArgs action) =>
    //        CollectionChanged?.Invoke(this, action);

    //    public event PropertyChangedEventHandler? PropertyChanged;

    //    public void OnPropertyChanged(string name) =>
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    //    #endregion

    //    #region List

    //    public TDto this[int index]
    //    {
    //        get => dtoList[index].Value;
    //        set => dtoList[index] = new(dtoList[index].Key, value);
    //    }

    //    public int Count => dtoList.Count;

    //    public bool IsReadOnly => false;

    //    /// <summary>
    //    /// 将指定的DOT添加到集合中
    //    /// 默认添加在最后的列表内
    //    /// </summary>
    //    /// <param name="item">要添加的DOT对象。</param>
    //    public void Add(TDto item)
    //    {
    //        int index = dtoList.Count;
    //        var list = entityList.GetLast();
    //        list?.Add(item.Entity);
    //        dtoList.Add(new(list, item));
    //        OnCollectionChanged(
    //            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
    //        );
    //    }

    //    /// <summary>
    //    /// 将指定的实体添加到集合中。
    //    /// </summary>
    //    /// <param name="entity">要添加的实体对象。</param>
    //    public void Add(TEntity entity)
    //    {
    //        ArgumentNullException.ThrowIfNull(entity);

    //        int index = dtoList.Count;
    //        var list = entityList.GetLast();
    //        ArgumentNullException.ThrowIfNull(list, "the entitiesList cannot be null or empty");
    //        list.Add(entity);
    //        TDto item = pool.Get();
    //        item.UpdateEntity(entity);
    //        dtoList.Add(new(list, item));
    //        OnCollectionChanged(
    //            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
    //        );
    //    }

    //    public void AddRange(IEnumerable<TEntity> entities)
    //    {
    //        var list = entityList.GetLast();
    //        ArgumentNullException.ThrowIfNull(list, "the entitiesList cannot be null or empty");
    //        foreach (var entity in entities)
    //        {
    //            list.Add(entity);
    //            TDto item = pool.Get();
    //            item.UpdateEntity(entity);
    //            dtoList.Add(new(list, item));
    //        }
    //        Refresh();
    //    }

    //    public void Clear()
    //    {
    //        DtoIndex = 0;
    //        ClearDotList();
    //        entityList.Clear();
    //        OnCollectionChanged(
    //            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
    //        );
    //    }

    //    /// <summary>
    //    /// 释放所有Dot对象，并清空列表
    //    /// 需要手动更新OnCollectionChanged
    //    /// </summary>
    //    private void ClearDotList()
    //    {
    //        for (int i = 0; i < dtoList.Count; i++)
    //        {
    //            pool.Release(dtoList[i].Value);
    //        }
    //        dtoList.Clear();
    //    }

    //    public bool Contains(TDto item)
    //    {
    //        return dtoList.FindIndex((d, v) => d.Value == v, item) > -1;
    //    }

    //    public bool Contains(TEntity item)
    //    {
    //        if (entityList.IsEmpty)
    //            return false;

    //        for (int i = 0; i < entityList.Count; i++)
    //        {
    //            if (entityList[i].Contains(item))
    //                return true;
    //        }
    //        return false;
    //    }

    //    public void CopyTo(TDto[] array, int arrayIndex)
    //    {
    //        //无法装下全部数据
    //        if (array.Length - arrayIndex > Count)
    //            throw new ArgumentException(nameof(array));

    //        for (int i = 0; i < array.Length; i++)
    //        {
    //            array[i + arrayIndex] = this[i];
    //        }
    //    }

    //    public int IndexOf(TDto item)
    //    {
    //        return dtoList.FindIndex((d, v) => d.Value == v, item);
    //    }

    //    public int IndexOf(TEntity item)
    //    {
    //        if (entityList.IsEmpty)
    //            return -1;

    //        for (int i = 0; i < entityList.Count; i++)
    //        {
    //            if (entityList[i].Contains(item))
    //                return i;
    //        }
    //        return -1;
    //    }

    //    public void Insert(int index, TDto item)
    //    {
    //        Insert(index, item, null);
    //    }

    //    public void Insert(int index, TDto item, List<TEntity>? entities = null)
    //    {
    //        dtoList.Insert(index, new KeyValuePair<List<TEntity>?, TDto>(entities, item));
    //        OnCollectionChanged(
    //            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index)
    //        );
    //    }

    //    public void Insert(int listIndex, int index, TEntity entity)
    //    {
    //        entityList[listIndex].Insert(index, entity);
    //    }

    //    public void InsertList(int index, List<TEntity> list)
    //    {
    //        entityList.Insert(index, list);
    //    }

    //    public bool Remove(TDto item)
    //    {
    //        if (item is null)
    //            return false;

    //        return Remove(item.Entity);
    //    }

    //    public bool Remove(TEntity entity)
    //    {
    //        if (entity is null)
    //            return false;

    //        int index = -1;

    //        if (!entityList.IsEmpty)
    //        {
    //            for (int i = 0; i < entityList.Count; i++)
    //            {
    //                if (entityList[i].Remove(entity))
    //                    break;
    //            }
    //        }

    //        index = dtoList.FindIndex((k, v) => k.Value.Entity == v, entity);

    //        if (index >= 0)
    //        {
    //            TDto item = dtoList[index].Value;
    //            dtoList.RemoveAt(index);
    //            pool.Release(item);
    //            OnCollectionChanged(
    //                new NotifyCollectionChangedEventArgs(
    //                    NotifyCollectionChangedAction.Remove,
    //                    item,
    //                    index
    //                )
    //            );
    //            return true; // 返回true表示成功移除了元素
    //        }
    //        return false; // 返回false表示元素不存在于集合中
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        if (index >= 0 && index < dtoList.Count)
    //        {
    //            TDto item = dtoList[index].Value;
    //            dtoList.RemoveAt(index);
    //            if (!entityList.IsEmpty)
    //            {
    //                for (int i = 0; i < entityList.Count; i++)
    //                {
    //                    if (entityList[i].Remove(item.Entity))
    //                        break;
    //                }
    //            }
    //            pool.Release(item);
    //            OnCollectionChanged(
    //                new NotifyCollectionChangedEventArgs(
    //                    NotifyCollectionChangedAction.Remove,
    //                    item,
    //                    index
    //                )
    //            );
    //        }
    //        else
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
    //        }
    //    }

    //    public IEnumerator<TDto> GetEnumerator()
    //    {
    //        for (int i = 0; i < dtoList.Count; i++)
    //        {
    //            yield return dtoList[i].Value;
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        for(int i = 0; i < Count; i++)
    //        {
    //            yield return dtoList[i].Value;
    //        }
    //    }

    //    #endregion

    //    #region Page

    //    public int GetAllEntitiesCount()
    //    {
    //        int index = 0;
    //        for (int i = 0; i < entityList.Count; i++)
    //        {
    //            index += entityList[i].Count;
    //        }
    //        return index;
    //    }

    //    public void UpdatePageSize(int pageSize)
    //    {
    //        if (pageSize <= 0) 
    //            throw new IndexOutOfRangeException(nameof(UpdatePageSize));
    //        PageSize = pageSize;
    //    }

    //    //public bool UpdateList(List<TEntity> entities)
    //    //{
    //    //    if (entities == null) return false;


    //    //    for (int i = 0; i < dtoList.Count; i++)
    //    //    {
    //    //        pool.Release(dtoList[i].Value);
    //    //    }
    //    //    dtoList.Clear();

    //    //    foreach (var entity in entities)
    //    //    {
    //    //        TDto item = pool.Get();
    //    //        item.EntityChanged(entity);
    //    //        dtoList.Add(item);
    //    //    }
    //    //    entityList = entities;
    //    //    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //    //    return true;
    //    //}

    //    public void AddEntitiesList(List<TEntity> entities)
    //    {
    //        ArgumentNullException.ThrowIfNull(entities);
    //        entityList.Add(entities);
    //    }

    //    public void RemoveEntitiesList(List<TEntity> entities)
    //    {
    //        ArgumentNullException.ThrowIfNull(entities);
    //        entityList.Remove(entities);
    //    }

    //    public void Refresh(int startIndex)
    //    {
    //        if (startIndex < 0)
    //            throw new ArgumentNullException(nameof(Refresh));

    //        DtoIndex = startIndex;

    //        Refresh();
    //    }

    //    public void Refresh(int pageSize, int pageIndex)
    //    {
    //        if (pageSize <= 0 || pageIndex < 0) 
    //            throw new ArgumentNullException(nameof(Refresh));

    //        DtoIndex = pageIndex;
    //        PageSize = pageSize;

    //        Refresh();
    //    }

    //    public void Refresh()
    //    {
    //        int allCount = GetAllEntitiesCount();

    //        // 检查当前页码是否有效
    //        if (DtoIndex < 0)
    //        {
    //            return;
    //        }

    //        ClearDotList();

    //        // 计算当前页应包含的数据范围
    //        int endIndex = Math.Min(DtoIndex + PageSize, allCount);
    //        int startIndex = DtoIndex;
    //        if (endIndex == allCount)
    //        {
    //            //如果结束函数已经到最后了，就不在移动
    //            startIndex = Math.Max(0, allCount - PageSize);
    //        }

    //        SetPageData(startIndex, endIndex);

    //        OnCollectionChanged(
    //            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
    //        );
    //    }

    //    /// <summary>
    //    /// 设置分页数据
    //    /// </summary>
    //    /// <param name="startIndex">起始索引</param>
    //    /// <param name="endIndex">结束索引</param>
    //    private void SetPageData(int startIndex, int endIndex)
    //    {
    //        int currentIndex = 0;
    //        for (int i = 0; i < entityList.Count; i++)
    //        {
    //            var list = entityList[i];
    //            currentIndex += list.Count;
    //            //小于当前需要到达的位置
    //            if (currentIndex <= startIndex)
    //            {
    //                continue;
    //            }
    //            else if (currentIndex > startIndex)
    //            {
    //                //在这个全在这个List内或者从这个列表开始
    //                ProcessPageData(i, startIndex, endIndex, currentIndex - list.Count);
    //                break;
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 处理分页数据
    //    /// </summary>
    //    /// <param name="listIndex">列表索引</param>
    //    /// <param name="startIndex">起始索引</param>
    //    /// <param name="endIndex">结束索引</param>
    //    /// <param name="currentIndex">当前索引</param>
    //    private void ProcessPageData(int listIndex, int startIndex, int endIndex, int currentIndex)
    //    {
    //        int range = endIndex - startIndex;
    //        int startindexOffset = startIndex - currentIndex;
    //        int endindexOffset = startindexOffset + range;
    //        int listCount = entityList[listIndex].Count;

    //        if (endindexOffset > listCount)
    //        {
    //            //如果还有数据在后面的列表中
    //            EntityToDto(listIndex, startindexOffset, listCount);
    //            int addCount = listCount - startindexOffset;
    //            ProcessPageData(listIndex++, startIndex + addCount, endIndex, currentIndex + addCount);
    //        }
    //        else
    //        {
    //            EntityToDto(listIndex, startindexOffset, endindexOffset);
    //        }
    //    }

    //    /// <summary>
    //    /// 将实体类转换为Dto类
    //    /// </summary>
    //    /// <param name="listIndex">列表索引</param>
    //    /// <param name="startIndex">起始索引</param>
    //    /// <param name="endIndex">结束索引</param>
    //    /// <exception cref="IndexOutOfRangeException">索引超出范围时抛出</exception>
    //    private void EntityToDto(int listIndex, int startIndex, int endIndex)
    //    {
    //        if (listIndex < 0 || listIndex > entityList.Count - 1 || startIndex < 0 || endIndex <= 0 || startIndex > endIndex)
    //            throw new IndexOutOfRangeException(nameof(EntityToDto));

    //        var list = entityList[listIndex];

    //        if (startIndex > list.Count || endIndex > list.Count)
    //            throw new IndexOutOfRangeException(nameof(EntityToDto));

    //        for (int i = startIndex; i < endIndex; i++)
    //        {
    //            TDto item = pool.Get();
    //            item.UpdateEntity(list[i]);
    //            dtoList.Add(new(list, item));
    //        }
    //    }

    //    #endregion
    //}
}
