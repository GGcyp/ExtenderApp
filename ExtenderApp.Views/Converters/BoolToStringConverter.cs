using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExtenderApp.Views.Converters
{
    public class BoolToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return string.Empty;

            if (parameter is not string param)
                return ToBoolString(boolValue);

            return param switch
            {
                "Inverse" => ToBoolString(!boolValue),
                "YesNo" => boolValue ? "Yes" : "No",
                "TrueFalse" => boolValue ? "True" : "False",
                _ => ToBoolString(boolValue)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        private string ToBoolString(bool value)
        {
            return value ? "是" : "否";
        }
    }
}
