using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary;
using ExtenderApp.Common.Serializations.Json;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    public static class SerializationExtensions
    {
        public static IServiceCollection AddSerializations(this IServiceCollection services)
        {
            services.AddSingleton<IJsonSerialization, JsonSerialization>();
            services.AddBinary();
            return services;
        }

        public static void Serialize<T>(this ISerialization serialization, T obj, IFileOperate operate)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);
            serialization.Serialize(obj, out ByteBuffer buffer);
            operate.Write(buffer);
            buffer.Dispose();
        }

        public static T? Deserialize<T>(this ISerialization serialization, IFileOperate operate)
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(operate);

            operate.Read(out ByteBuffer buffer);
            var result = serialization.Deserialize<T>(buffer.UnreadSequence);
            buffer.Dispose();
            return result;
        }
    }
}