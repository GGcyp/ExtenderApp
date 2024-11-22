using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ExtenderApp.Mod.PPR
{
    /// <summary>
    /// FrequencyView.xaml 的交互逻辑
    /// 用于PPR的期数输入
    /// </summary>
    public partial class FrequencyView : Window
    {
        public Action<string> FrequencyChanged;

        public FrequencyView(Action<string> action)
        {
            InitializeComponent();
            FrequencyChanged = action;
            //textBox_Frequency.PreviewTextInput += TextBox_Frequency_PreviewTextInput;
        }

        //private void TextBox_Frequency_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    //// 只允许输入数字（包括小数点，如果需要的话）
        //    //// 注意：这个示例只允许一个小数点，并且它必须位于数字的开头之后
        //    //Regex regex = new Regex("^[0-9]*(\\.[0-9]{0,1})?$|^(\\.)?[0-9]+$");
        //    //e.Handled = !regex.IsMatch(sender.ToString() + e.Text);

        //    //如果你不需要小数点，可以使用更简单的正则表达式
        //    Regex regex = new Regex("^[0-9]*$");
        //    e.Handled = !regex.IsMatch(sender.ToString().Insert(textBox_Frequency.Text.Length, e.Text));
        //}

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入数字（包括小数点，如果需要的话）
            // 注意：这个示例只允许一个小数点，并且它必须位于数字的开头之后
            Regex regex = new Regex("^[0-9]*(\\.[0-9]{0,1})?$|^(\\.)?[0-9]+$");
            e.Handled = !regex.IsMatch(sender.ToString() + e.Text);

            // 如果你不需要小数点，可以使用更简单的正则表达式
            // Regex regex = new Regex("^[0-9]*$");
            // e.Handled = !regex.IsMatch(sender.ToString().Insert(sender.Text.Length, e.Text));

            // 注意：上面的正则表达式有一个问题，它在文本框为空时允许输入小数点。
            // 你可以通过添加一些额外的逻辑来处理这个问题，比如：
            // if (string.IsNullOrEmpty(((TextBox)sender).Text) && e.Text == ".")
            // {
            //     e.Handled = true; // 不允许在空文本框中输入小数点
            // }
            // 然后在正则表达式中去掉对小数点的特殊处理。
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            FrequencyChanged?.Invoke(textBox_Frequency.Text);
            Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancellationButton_Click(object sender, RoutedEventArgs e)
        {
            FrequencyChanged?.Invoke(string.Empty);
            Close();
        }
    }
}
