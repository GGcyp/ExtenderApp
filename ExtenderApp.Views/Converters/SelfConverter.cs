using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    /// <summary>
    /// SelfConverter 类是一个自定义的值转换器，它实现了 MarkupExtension 和 IValueConverter 接口。
    /// </summary>
    public class SelfConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 将值从源类型转换为目标类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">转换器参数（如果有）。</param>
        /// <param name="culture">所用区域性信息。</param>
        /// <returns>转换后的值。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        /// <summary>
        /// 将值从目标类型转换回源类型。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">转换器参数（如果有）。</param>
        /// <param name="culture">所用区域性信息。</param>
        /// <returns>转换后的值。</returns>
        /// <exception cref="NotImplementedException">此方法尚未实现。</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 提供标记扩展的值。
        /// </summary>
        /// <param name="serviceProvider">服务提供程序。</param>
        /// <returns>标记扩展的值。</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
