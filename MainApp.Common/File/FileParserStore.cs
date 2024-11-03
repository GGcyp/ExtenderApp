using System.Collections;
using MainApp.Common.Data;

namespace MainApp.Common.File
{
    internal class FileParserStore : IList<IFileParser>
    {
        private ValueList<IFileParser> parsers;

        public FileParserStore(IEnumerable<IFileParser> parsers)
        {
            this.parsers = new ValueList<IFileParser>(parsers);
        }

        IFileParser IList<IFileParser>.this[int index] 
        {
            get => parsers[index];
            set => parsers[index] = value; 
        }

        public int Count => parsers.Count;

        public bool IsReadOnly => false;

        public void Clear()
        {
            parsers.Clear();
        }

        public void CopyTo(IFileParser[] array, int arrayIndex)
        {
            parsers.CopyTo(array, arrayIndex);
        }

        public IEnumerator GetEnumerator()
        {
            return parsers.GetEnumerator();
        }

        public void RemoveAt(int index)
        {
            parsers.RemoveAt(index);
        }

        public void Add(object? item)
        {
            if (item == null) throw new ArgumentNullException("item");

            if (!(item is IFileParser fileParser)) throw new ArgumentNullException(nameof(IFileParser));

            Add(fileParser);
        }

        void ICollection<IFileParser>.Add(IFileParser item)
        {
            if (parsers.Contains(item)) throw new InvalidOperationException(nameof(item.ExtensionType.Extension));

            parsers.Add(item);
        }

        bool ICollection<IFileParser>.Contains(IFileParser item)
        {
            return parsers.Contains(item);
        }

        IEnumerator<IFileParser> IEnumerable<IFileParser>.GetEnumerator()
        {
            return parsers.GetEnumerator();
        }

        int IList<IFileParser>.IndexOf(IFileParser item)
        {
            return parsers.IndexOf(item);
        }

        void IList<IFileParser>.Insert(int index, IFileParser item)
        {
            parsers.Insert(index, item);
        }

        bool ICollection<IFileParser>.Remove(IFileParser item)
        {
            return parsers.Remove(item);
        }
    }
}
