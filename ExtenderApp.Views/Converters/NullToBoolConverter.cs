using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    /// <summary>
    /// NullToBoolConverter 类，用于将 null 值转换为布尔值。
    /// </summary>
    public class NullToBoolConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 将值转换为布尔值。m
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">转换参数，可选。如果参数为 "Inverse"，则在值为 null 时返回 true。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>如果值为 null，则返回 false；否则返回 true。如果参数为 "Inverse"，则反转结果。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 可选：通过 parameter 反转结果（如 "Inverse" 表示 null 时返回 true）
            bool inverse = false;
            if (parameter is string i)
            {
                inverse = i.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase);
            }
            bool result = value != null;
            return inverse ? !result : result;
        }

        /// <summary>
        /// 不实现反向转换功能，抛出异常。
        /// </summary>
        /// <param name="value">要转换的值。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">转换参数。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>抛出 <see cref="NotImplementedException"/> 异常，因为这是一个单向转换器。</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 单向转换，无需实现
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
