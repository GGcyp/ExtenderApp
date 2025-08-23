using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    /// <summary>
    /// 将布尔值转换为Visibility枚举的转换器
    /// </summary>
    public class BooleanToVisibilityConverter : MarkupExtension,IValueConverter
    {
        /// <summary>
        /// 将bool值转换为Visibility
        /// </summary>
        /// <param name="value">绑定源的值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数：可指定"Inverse"实现反向转换</param>
        /// <param name="culture">区域信息</param>
        /// <returns>Visible或Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 默认为正向转换：true -> Visible，false -> Collapsed
            bool isVisible = (bool)value;

            if(!(parameter is string param))
                return isVisible ? Visibility.Visible : Visibility.Collapsed;

            // 检查是否需要反向转换
            if (param.Contains("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible; 
            }
            if (param.Contains("Hidden", StringComparison.OrdinalIgnoreCase))
            {
                return isVisible? Visibility.Visible: Visibility.Hidden;
            }


            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 将Visibility转换回bool值（反向转换）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            bool result = visibility == Visibility.Visible;

            // 检查是否需要反向转换
            if (parameter is string param && param.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                result = !result;
            }

            return result;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
