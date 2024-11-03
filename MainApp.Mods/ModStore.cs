using MainApp.Common.Data;

namespace MainApp.Mods
{
    /// <summary>
    /// 模型详情存储
    /// </summary>
    public class ModStore : Store<ModDetails>
    {
        public ModStore(int capacity) : base(capacity)
        {

        }

        public ModStore()
        {

        }
    }
}
