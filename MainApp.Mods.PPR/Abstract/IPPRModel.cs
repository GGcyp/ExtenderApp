using MainApp.Abstract;

namespace MainApp.Mods.PPR
{
    public interface IPPRModel : IModel<PPRDto>
    {
        /// <summary>
        /// 根据属性名称获取对应的PPREntity对象。
        /// </summary>
        /// <param name="proName">属性名称。</param>
        /// <returns>找到的PPREntity对象，若未找到则返回null。</returns>
        PPRDto? Get(string proName);
        /// <summary>
        /// 根据名称移除对应的PPREntity对象。
        /// </summary>
        /// <param name="name">PPREntity对象的名称。</param>
        /// <returns>被移除的PPREntity对象，若未找到则返回null。</returns>
        PPRDto? Remove(string proName);

        /// <summary>
        /// 更新工程量标号为-1的每期数据
        /// </summary>
        /// <param name="frequency">更新的期数</param>
        void UpdatePerPeriodQuantityEntityFrequency(int frequency);
    }
}
