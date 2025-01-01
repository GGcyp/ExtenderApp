

namespace ExtenderApp.Data
{
    /// <summary>
    /// 泛型字典存储类，继承自<see cref="Dictionary{TKey, TValue}"/>。
    /// </summary>
    /// <typeparam name="TKey">键的类型，必须是非空类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    public class DictionaryStore<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
    {
    }
}
