using System.Windows;
using System.Windows.Input;

namespace ExtenderApp.Views.Commands
{
    /// <summary>
    /// 简单的命令实现，适用于无需命令参数或使用 object 参数的场景。 实现 <see cref="ICommand"/>，自动将 <see cref="CanExecuteChanged"/> 与 <see
    /// cref="CommandManager.RequerySuggested"/> 关联。
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// 要执行的操作（接受可空 object 参数）。
        /// </summary>
        private readonly Action<object?> _execute;

        /// <summary>
        /// 判断命令是否可执行的谓词（接受可空 object 参数），可为 null 表示始终可执行。
        /// </summary>
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// 当命令的可执行状态可能发生变化时触发的事件。 此实现将订阅/取消订阅到 <see cref="CommandManager.RequerySuggested"/>。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 使用无参 <see cref="Action"/> 初始化一个新的 <see cref="RelayCommand"/> 实例。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        public RelayCommand(Action execute) : this(execute, null!)
        {
        }

        /// <summary>
        /// 使用无参操作与无参判断函数初始化一个新的 <see cref="RelayCommand"/> 实例。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        /// <param name="canExecute">判断是否可执行的函数（返回 bool）。</param>
        public RelayCommand(Action execute, Func<bool> canExecute) : this(o => execute.Invoke(), canExecute == null ? null : o => canExecute.Invoke())
        {
        }

        /// <summary>
        /// 使用带参数的操作初始化一个新的 <see cref="RelayCommand"/> 实例。
        /// </summary>
        /// <param name="execute">要执行的操作，接受 object? 参数。</param>
        public RelayCommand(Action<object?> execute) : this(execute, null!)
        {
        }

        /// <summary>
        /// 使用带参数的操作与参数判断谓词初始化一个新的 <see cref="RelayCommand"/> 实例。
        /// </summary>
        /// <param name="execute">要执行的操作。</param>
        /// <param name="canExecute">判断是否可执行的谓词。</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令在指定参数下是否可以执行。
        /// </summary>
        /// <param name="parameter">命令参数，允许为 null。</param>
        /// <returns>如果可以执行则返回 true，否则 false。</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// 执行命令逻辑。
        /// </summary>
        /// <param name="parameter">命令参数，允许为 null。</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public static implicit operator RelayCommand<object>(RelayCommand command)
            => new RelayCommand<object>(command.Execute, command.CanExecute);

        public static implicit operator RelayCommand(RelayCommand<object> command)
            => new RelayCommand(command.Execute, command.CanExecute);

        public static implicit operator RelayCommand(Action command)
            => new RelayCommand(command);
    }

    /// <summary>
    /// 泛型版本的 <see cref="RelayCommand"/>, 可接受强类型参数 <typeparamref name="T"/>。 在 WPF XAML 中传入 FrameworkElement 时，会尝试使用其 DataContext 作为参数源。
    /// </summary>
    /// <typeparam name="T">命令参数的类型。</typeparam>
    public class RelayCommand<T> : ICommand
    {
        /// <summary>
        /// 执行动作，接受 <typeparamref name="T"/> 类型参数。
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// 判断是否可执行的谓词，接受 <typeparamref name="T"/> 类型参数。
        /// </summary>
        private readonly Predicate<T>? _canExecute;

        /// <summary>
        /// 当命令的可执行状态可能发生变化时触发的事件。 此实现将订阅/取消订阅到 <see cref="CommandManager.RequerySuggested"/>。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 使用执行动作初始化一个新的 <see cref="RelayCommand{T}"/> 实例。
        /// </summary>
        /// <param name="execute">执行动作，不能为空。</param>
        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        /// <summary>
        /// 使用执行动作与可执行判断初始化一个新的 <see cref="RelayCommand{T}"/> 实例。
        /// </summary>
        /// <param name="execute">执行动作，不能为空。</param>
        /// <param name="canExecute">可执行判断，可为 null 表示始终可执行。</param>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 执行命令。参数可为：
        /// - 直接为 <typeparamref name="T"/>：直接传入；
        /// - 为 <see cref="FrameworkElement"/>：尝试使用其 <see cref="FrameworkElement.DataContext"/> 作为 <typeparamref name="T"/>。 若都不符合，若 <typeparamref
        ///   name="T"/> 为引用类型会传入 null，为值类型会传入 default。
        /// </summary>
        /// <param name="parameter">命令参数（object?）。</param>
        public void Execute(object? parameter)
        {
            switch (parameter)
            {
                case T result:
                    _execute(result);
                    break;

                case FrameworkElement el:
                    if (el.DataContext is T data)
                    {
                        _execute(data);
                        break;
                    }
                    _execute(default!);
                    break;

                default:
                    _execute(default!);
                    break;
            }
        }

        /// <summary>
        /// 强类型执行方法，直接接受 <typeparamref name="T"/> 参数。
        /// </summary>
        /// <param name="parameter">强类型参数。</param>
        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// 判断命令在指定参数下是否可以执行。 支持直接传入 <typeparamref name="T"/> 或者传入包含目标数据的 <see cref="FrameworkElement"/>（使用其 DataContext）。
        /// </summary>
        /// <param name="parameter">命令参数（object?）。</param>
        /// <returns>如果命令可以执行则返回 true，否则 false。</returns>
        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null) return true;

            return parameter switch
            {
                T result => _canExecute(result),
                FrameworkElement el => ElementCanExecute(el),
                _ => true,
            };

            bool ElementCanExecute(FrameworkElement el)
            {
                if (el.DataContext is not T result)
                    return true;

                return _canExecute(result);
            }
        }

        /// <summary>
        /// 强类型的可执行判断。
        /// </summary>
        /// <param name="parameter">强类型参数。</param>
        /// <returns>如果命令可以执行则返回 true，否则 false。</returns>
        public bool CanExecute(T parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute(parameter);
        }
    }
}