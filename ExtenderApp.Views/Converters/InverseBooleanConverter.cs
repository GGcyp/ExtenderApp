using System.Globalization; 
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    /// <summary>
    /// 布尔值反转转换器类
    /// </summary>
    public class InverseBooleanConverter : MarkupExtension,IValueConverter
    {
        /// <summary>
        /// 将输入值转换为输出值
        /// </summary>
        /// <param name="value">输入值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域信息</param>
        /// <returns>反转后的布尔值或false</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return false;
        }

        /// <summary>
        /// 将输出值转换回输入值
        /// </summary>
        /// <param name="value">输出值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域信息</param>
        /// <returns>未实现异常</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}