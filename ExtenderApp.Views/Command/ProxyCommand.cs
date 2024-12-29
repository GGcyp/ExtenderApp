using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 表示一个代理命令，实现了ICommand接口
    /// </summary>
    public class ProxyCommand : ICommand
    {
        /// <summary>
        /// 获取执行操作的委托的工厂方法
        /// </summary>
        private readonly Func<Action<object>> _getExecute;

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// 当命令的执行状态更改时发生的事件
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 使用执行操作的工厂方法初始化ProxyCommand类的新实例
        /// </summary>
        /// <param name="execute">执行操作的工厂方法</param>
        public ProxyCommand(Func<Action<object>> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// 使用执行操作的工厂方法和判断命令是否可以执行的谓词初始化ProxyCommand类的新实例
        /// </summary>
        /// <param name="execute">执行操作的工厂方法</param>
        /// <param name="canExecute">判断命令是否可以执行的谓词</param>
        public ProxyCommand(Func<Action<object>> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _getExecute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 定义命令是否可以执行的方法
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果命令可以执行，则返回true；否则返回false</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// 定义执行命令的方法
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object parameter)
        {
            var execute = _getExecute.Invoke();
            execute?.Invoke(parameter);
        }
    }
}
