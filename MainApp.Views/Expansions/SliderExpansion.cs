using System.Windows.Controls;

namespace MainApp.Views.Expansions
{
    /// <summary>
    /// 提供Slider控件的扩展方法
    /// </summary>
    public static class SliderExpansion
    {
        /// <summary>
        /// 设置Slider控件的最小值、最大值、当前值和步长
        /// </summary>
        /// <param name="slider">Slider控件实例</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="value">当前值</param>
        /// <param name="step">步长</param>
        /// <returns>返回设置后的Slider控件实例</returns>
        /// <exception cref="ArgumentNullException">当slider为null时抛出此异常</exception>
        public static Slider SetValue(this Slider slider, double minValue, double maxValue, double value, double step)
        {
            ArgumentNullException.ThrowIfNull(slider, nameof(slider));

            slider.Minimum = minValue;
            slider.Maximum = maxValue;
            slider.Value = value;
            slider.LargeChange = step;
            slider.SmallChange = step;
            return slider;
        }
    }
}
