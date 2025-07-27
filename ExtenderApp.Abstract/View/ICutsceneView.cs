

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 切场视图接口
    /// </summary>
    public interface ICutsceneView : IView
    {
        /// <summary>
        /// 开始切场
        /// </summary>
        void Start();

        /// <summary>
        /// 结束切场
        /// </summary>
        void End();
    }
}
