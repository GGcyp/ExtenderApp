using System.Collections.ObjectModel;


namespace ExtenderApp.Data
{
    /// <summary>
    /// 继承自 ObservableCollection<T> 的可观察存储类。
    /// </summary>
    /// <typeparam name="T">存储元素的类型。</typeparam>
    public class ObservableStore<T> : ObservableCollection<T>
    {
    }
}
