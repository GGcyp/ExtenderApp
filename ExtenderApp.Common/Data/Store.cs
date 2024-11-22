using System.Collections;

namespace ExtenderApp.Common.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Store<T> : List<T>
    {
        public Store(int capacity) : base(capacity)
        {
            
        }

        public Store(IEnumerable<T> collection) : base(collection)
        {

        }

        public Store() : base()
        {

        }

        //protected ValueList<T> values;

        //public virtual T this[int index] 
        //{ 
        //    get => values[index];
        //    set => values[index] = value; 
        //}

        //public int Count => values.Count;

        //public virtual bool IsReadOnly => false;

        //public Store(int capacity)
        //{
        //    values = new ValueList<T>(capacity);
        //}

        //public Store(IEnumerable<T> collection)
        //{
        //    values = new(collection);
        //}

        //public Store()
        //{
        //    values = new ();
        //}

        //public virtual void Add(T item)
        //{
        //    values.Add(item);
        //}

        //public virtual void Clear()
        //{
        //    values.Clear();
        //}

        //public virtual bool Contains(T item)
        //{
        //    return values.Contains(item);
        //}

        //public void CopyTo(T[] array, int arrayIndex)
        //{
        //    values.CopyTo(array, arrayIndex);
        //}

        //public IEnumerator<T> GetEnumerator()
        //{
        //    return values.GetEnumerator();
        //}

        //public virtual int IndexOf(T item)
        //{
        //    return values.IndexOf(item);
        //}

        //public virtual void Insert(int index, T item)
        //{
        //    values.Insert(index, item);
        //}

        //public virtual bool Remove(T item)
        //{
        //    return values.Remove(item);
        //}

        //public virtual void RemoveAt(int index)
        //{
        //    values.RemoveAt(index);
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
    }
}
