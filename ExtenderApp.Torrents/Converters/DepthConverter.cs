using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Torrents.Converters
{
    /// <summary>
    /// DepthConverter 类用于将深度值转换为边距。
    /// </summary>
    public class DepthConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 将输入值转换为指定的目标类型。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">转换器参数。</param>
        /// <param name="culture">用于转换的区域性。</param>
        /// <returns>转换后的对象。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int depth = (int)value;
            int depthLength = 21;

            if (parameter is string depthLengthString)
            {
                depthLength = int.Parse(depthLengthString);
            }

            Thickness margin = new Thickness(depth * depthLength, 0, 0, 0);
            if (depth == 0)
                margin = new Thickness(5, 0, 0, 0);
            return margin;
        }

        /// <summary>
        /// 将目标类型的值转换回原始类型。
        /// </summary>
        /// <param name="value">目标值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">转换器参数。</param>
        /// <param name="culture">用于转换的区域性。</param>
        /// <returns>转换后的对象。</returns>
        /// <exception cref="NotImplementedException">此方法未实现。</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 提供值。
        /// </summary>
        /// <param name="serviceProvider">服务提供程序。</param>
        /// <returns>提供的值。</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
