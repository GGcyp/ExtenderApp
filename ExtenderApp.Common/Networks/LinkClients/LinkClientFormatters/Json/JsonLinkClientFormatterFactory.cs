using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal class JsonLinkClientFormatterFactory
    {
        private readonly IBinaryFormatter<string> _stringFormatter;

        public JsonLinkClientFormatterFactory(IBinaryFormatter<string> stringFormatter)
        {
            _stringFormatter = stringFormatter;
        }

        public JsonLinkClientFormatter<T> CreateFormatter<T>()
        {
            return new JsonLinkClientFormatter<T>(_stringFormatter);
        }
    }
}
