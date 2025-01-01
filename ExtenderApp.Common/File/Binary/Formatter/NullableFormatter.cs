using ExtenderApp.Abstract;
using ExtenderApp.Data;
using BinaryWriter = ExtenderApp.Data.BinaryWriter;

namespace ExtenderApp.Common.File.Binary
{
    public sealed class NullableFormatter<T> : IBinaryFormatter<T?> where T : struct
    {
        private readonly IBinaryFormatterResolver _formattable;

        public NullableFormatter(IBinaryFormatterResolver formattable)
        {
            _formattable = formattable;
        }

        public T? Deserialize(ref Data.BinaryReader reader)
        {
            //    if (reader.IsNil)
            //    {
            //        reader.ReadNil();
            //        return null;
            //    }
            //    else
            //    {
            //        return options.Resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
            //    }
            return null;
        }

        public void Serialize(ref BinaryWriter writer, T? value)
        {
            if (value == null)
            {
                //writer.WriteNil();
            }
            else
            {
                _formattable.GetFormatterWithVerify<T>().Serialize(ref writer, value.Value);
            }
        }
    }

    public sealed class StaticNullableFormatter<T> : IBinaryFormatter<T?> where T : struct
    {
        private readonly IBinaryFormatter<T> _underlyingFormatter;

        public StaticNullableFormatter(IBinaryFormatter<T> underlyingFormatter)
        {
            _underlyingFormatter = underlyingFormatter;
        }

        public T? Deserialize(ref Data.BinaryReader reader)
        {
            //    if (reader.TryReadNil())
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        return this.underlyingFormatter.Deserialize(ref reader, options);
            //    }
            return default;
        }

        public void Serialize(ref BinaryWriter writer, T? value)
        {
            if (value == null)
            {
                //writer.WriteNil();
            }
            else
            {
                _underlyingFormatter.Serialize(ref writer, value.Value);
            }
        }
    }
}
