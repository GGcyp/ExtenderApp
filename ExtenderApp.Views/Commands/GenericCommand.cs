using System.Windows.Input;

namespace ExtenderApp.Views.Commands
{
    /// <summary>
    /// 泛型命令类，实现了ICommand接口。
    /// </summary>
    /// <typeparam name="T">泛型类型参数，必须是类类型。</typeparam>
    public class GenericCommand<T> : ICommand where T : class
    {
        /// <summary>
        /// 要执行的操作。
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// 判断操作是否可执行。
        /// </summary>
        private readonly Predicate<T> _canExecute;

        /// <summary>
        /// 当 CanExecute 状态更改时触发的事件。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 初始化 GenericCommand 类的新实例，仅指定要执行的操作。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        public GenericCommand(Action<T> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// 初始化 GenericCommand 类的新实例，指定要执行的操作和判断操作是否可执行的谓词。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        /// <param name="canExecute">判断操作是否可执行的谓词。</param>
        public GenericCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果命令可以执行，则返回true；否则返回false</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            if (parameter is T generic) return _canExecute(generic);

            return false;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object parameter)
        {
            _execute(parameter as T);
        }
    }
}
