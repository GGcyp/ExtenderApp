namespace ExtenderApp.Data
{
    [Serializable]
    public abstract class LocalData
    {
        public Version Version { get; set; }

        public LocalData(Version version)
        {
            Version = version;
        }

        public abstract void SaveData(ExpectLocalFileInfo info);
    }

    [Serializable]
    public class LocalData<T> : LocalData
    {
        public Action<ExpectLocalFileInfo, Version, T> SaveAcion { get; set; }

        public T Data { get; }

        public LocalData(T data, Action<ExpectLocalFileInfo, Version, T> saveAction, Version version) : base(version)
        {
            Data = data;
            SaveAcion = saveAction;
        }

        public override void SaveData(ExpectLocalFileInfo info)
        {
            SaveAcion.Invoke(info, Version, Data);
        }
    }
}
