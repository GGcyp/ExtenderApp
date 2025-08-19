using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatters
{
    internal class IEnumerableFormatter<T> : ResolverFormatter<IEnumerable<T>>
    {
        private readonly IBinaryFormatter<T> _formatter;
        private readonly IBinaryFormatter<List<T>> _list;

        public override int DefaultLength => 5 + _formatter.DefaultLength;

        public IEnumerableFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = resolver.GetFormatter<T>();
            _list = resolver.GetFormatter<List<T>>();
        }

        public override IEnumerable<T> Deserialize(ref ExtenderBinaryReader reader)
        {
            var list = _list.Deserialize(ref reader);
            return list;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, IEnumerable<T> value)
        {
            var list = new List<T>(value);
            _list.Serialize(ref writer, list);
        }

        public override long GetLength(IEnumerable<T> value)
        {
            if (value == null)
                return 1;

            long length = 5;
            foreach (var item in value)
            {
                length += _formatter.GetLength(item);
            }
            return length;
        }
    }
}
