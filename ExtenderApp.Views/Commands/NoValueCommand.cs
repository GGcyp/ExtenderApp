using System.Windows.Input;

namespace ExtenderApp.Views.Commands
{
    /// <summary>
    /// 表示一个无返回值的命令
    /// </summary>
    public class NoValueCommand : ICommand
    {
        /// <summary>
        /// 要执行的委托
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        /// 判断是否可执行委托
        /// </summary>
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 当命令可执行状态改变时触发的事件
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 初始化 NoValueCommand 类的新实例，并指定要执行的委托
        /// </summary>
        /// <param name="execute">要执行的委托</param>
        public NoValueCommand(Action execute) : this(execute, null)
        {
        }

        /// <summary>
        /// 初始化 NoValueCommand 类的新实例，并指定要执行的委托和判断委托是否可执行的委托
        /// </summary>
        /// <param name="execute">要执行的委托</param>
        /// <param name="canExecute">判断委托是否可执行的委托</param>
        public NoValueCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 定义命令是否可执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果命令可执行，则为 true；否则为 false</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object parameter)
        {
            _execute.Invoke();
        }
    }


}
