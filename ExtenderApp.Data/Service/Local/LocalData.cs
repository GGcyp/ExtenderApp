namespace ExtenderApp.Data
{
    [Serializable]
    public class LocalData
    {
        public Version? Version { get; set; }

        public LocalData(Version version)
        {
            Version = version;
        }
    }

    [Serializable]
    public class LocalData<T> : LocalData
    {
        public T? Data { get; set; }

        public LocalData(T? data, Version version) : base(version)
        {
            Data = data;
        }

        public LocalData(T? data) : base(null)
        {
            Data = data;
        }
    }
}
