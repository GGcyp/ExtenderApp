using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Service
{
    internal class LocalDataService  : ILocalDataService
    {
        private IPathService _pathService;
        private ValueList<LocalFileInfo> localDataInfos;

        public LocalDataService(IPathService pathService)
        {
            _pathService = pathService;
            localDataInfos = new ();

            CheckLocalData();
        }

        public void CheckLocalData()
        {
            string dataPath = _pathService.DataPath;

            var dataPaths = Directory.GetFiles(dataPath);

            foreach(var path  in dataPaths)
            {
                //localDataInfos.Add(new LocalDataInfo() { DataPath = path });
            }
        }
    }
}
