using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExtenderApp.Views
{
    /// <summary>
    /// RelayCommand 类实现了 ICommand 接口，用于在 WPF 或 UWP 应用中处理命令。
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// 要执行的操作。
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// 判断操作是否可执行。
        /// </summary>
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// 当 CanExecute 状态更改时触发的事件。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 初始化 RelayCommand 类的新实例，仅指定要执行的操作。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// 初始化 RelayCommand 类的新实例，指定要执行的操作和判断操作是否可执行的谓词。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        /// <param name="canExecute">判断操作是否可执行的谓词。</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 确定命令是否可以在当前命令目标上执行。
        /// </summary>
        /// <param name="parameter">命令使用的数据。如果命令不需要参数，可以将其设置为 null。</param>
        /// <returns>如果可以在当前命令目标上执行此命令，则为 true；否则为 false。</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// 在命令目标上执行命令。
        /// </summary>
        /// <param name="parameter">命令使用的数据。如果命令不需要参数，可以将其设置为 null。</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
