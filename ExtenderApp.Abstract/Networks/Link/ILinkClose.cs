namespace ExtenderApp.Abstract
{
    /// <summary>
    /// link 关闭接口，表示实现类具有关闭链接资源的能力。
    /// </summary>
    public interface ILinkClose
    {
        /// <summary>
        /// 关闭连接并释放相关资源。
        /// </summary>
        void Close();
    }
}