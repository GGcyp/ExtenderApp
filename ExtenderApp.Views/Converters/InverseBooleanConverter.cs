using System.Globalization; 
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    /// <summary>
    /// ����ֵ��תת������
    /// </summary>
    public class InverseBooleanConverter : MarkupExtension,IValueConverter
    {
        /// <summary>
        /// ������ֵת��Ϊ���ֵ
        /// </summary>
        /// <param name="value">����ֵ</param>
        /// <param name="targetType">Ŀ������</param>
        /// <param name="parameter">����</param>
        /// <param name="culture">������Ϣ</param>
        /// <returns>��ת��Ĳ���ֵ��false</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return false;
        }

        /// <summary>
        /// �����ֵת��������ֵ
        /// </summary>
        /// <param name="value">���ֵ</param>
        /// <param name="targetType">Ŀ������</param>
        /// <param name="parameter">����</param>
        /// <param name="culture">������Ϣ</param>
        /// <returns>δʵ���쳣</returns>
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