using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;


namespace ExtenderApp.Common.Error
{
    /// <summary>
    /// 错误处理工具类
    /// </summary>
    public static class ErrorUtil
    {
        #region ArgumentOutOfRangeException

        /// <summary>
        /// 检查参数值是否在指定范围内
        /// </summary>
        /// <param name="condition">判断条件</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="message">错误信息（可选）</param>
        /// <exception cref="ArgumentOutOfRangeException">如果条件不满足，则抛出此异常</exception>
        [DebuggerStepThrough]
        public static void ArgumentOutOfRange(this bool condition, string parameterName, string? message = null)
        {
            if (condition) return;

            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        #endregion

        #region ArgumentNullException

        /// <summary>
        /// 检查对象是否为空
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">对象值</param>
        /// <param name="parameterName">参数名称</param>
        /// <exception cref="ArgumentNullException">如果对象为空，则抛出此异常</exception>
        [DebuggerStepThrough]
        public static T ArgumentNull<T>([NotNull] this T value, string parameterName) where T : class
        {
            if (value != null) return value;

            throw new ArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        public static object ArgumentObjectNull([NotNull] this object value, string parameterName)
        {
            if (value != null) return value;

            throw new ArgumentNullException(parameterName);
        }

        #endregion

        #region ArgumentException


        /// <summary>
        /// 判断条件是否为真，如果不为真则抛出异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <exception cref="ArgumentException">如果条件不为真，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentTure(this bool condition, string parameterName)
        {
            if (condition) return;

            throw new ArgumentException(parameterName);
        }

        /// <summary>
        /// 判断条件是否为真，如果不为真则抛出包含消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息。</param>
        /// <exception cref="ArgumentException">如果条件不为真，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentTure(this bool condition, string parameterName, string message)
        {
            if (condition) return;

            throw new ArgumentException(message, parameterName);
        }

        /// <summary>
        /// 判断条件是否为真，如果不为真则抛出包含格式化消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息格式。</param>
        /// <param name="arg1">格式化消息的第一个参数。</param>
        /// <exception cref="ArgumentException">如果条件不为真，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentTure(this bool condition, string parameterName, string message, object arg1)
        {
            if (condition) return;

            throw new ArgumentException(string.Format(message, arg1), parameterName);
        }

        /// <summary>
        /// 判断条件是否为真，如果不为真则抛出包含格式化消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息格式。</param>
        /// <param name="arg1">格式化消息的第一个参数。</param>
        /// <param name="arg2">格式化消息的第二个参数。</param>
        /// <exception cref="ArgumentException">如果条件不为真，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentTure(this bool condition, string parameterName, string message, object arg1, object arg2)
        {
            if (condition) return;

            throw new ArgumentException(String.Format(message, arg1, arg2), parameterName);
        }

        /// <summary>
        /// 判断条件是否为真，如果不为真则抛出包含格式化消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息格式。</param>
        /// <param name="args">格式化消息的参数数组。</param>
        /// <exception cref="ArgumentException">如果条件不为真，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentTure(this bool condition, string parameterName, string message, params object[] args)
        {
            if (condition) return;

            throw new ArgumentException(String.Format(message, args), parameterName);
        }

        /// <summary>
        /// 判断条件是否为假，如果不为假则抛出异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <exception cref="ArgumentException">如果条件不为假，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentFalse(this bool condition, string parameterName)
        {
            if (!condition) return;

            throw new ArgumentException(parameterName);
        }

        /// <summary>
        /// 判断条件是否为假，如果不为假则抛出包含消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息。</param>
        /// <exception cref="ArgumentException">如果条件不为假，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentFalse(this bool condition, string parameterName, string message)
        {
            if (!condition) return;

            throw new ArgumentException(message, parameterName);
        }

        /// <summary>
        /// 判断条件是否为假，如果不为假则抛出包含格式化消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息格式。</param>
        /// <param name="arg1">格式化消息的第一个参数。</param>
        /// <exception cref="ArgumentException">如果条件不为假，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentFalse(this bool condition, string parameterName, string message, object arg1)
        {
            if (!condition) return;

            throw new ArgumentException(string.Format(message, arg1), parameterName);
        }

        /// <summary>
        /// 判断条件是否为假，如果不为假则抛出包含格式化消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息格式。</param>
        /// <param name="arg1">格式化消息的第一个参数。</param>
        /// <param name="arg2">格式化消息的第二个参数。</param>
        /// <exception cref="ArgumentException">如果条件不为假，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentFalse(this bool condition, string parameterName, string message, object arg1, object arg2)
        {
            if (!condition) return;

            throw new ArgumentException(String.Format(message, arg1, arg2), parameterName);
        }

        /// <summary>
        /// 判断条件是否为假，如果不为假则抛出包含格式化消息的异常。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="parameterName">参数名。</param>
        /// <param name="message">异常消息格式。</param>
        /// <param name="args">格式化消息的参数数组。</param>
        /// <exception cref="ArgumentException">如果条件不为假，则抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentFalse(this bool condition, string parameterName, string message, params object[] args)
        {
            if (!condition) return;

            throw new ArgumentException(String.Format(message, args), parameterName);
        }

        #endregion

        #region InvalidOperationException

        /// <summary>
        /// 对给定的值执行操作。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="value">要操作的值。</param>
        /// <param name="message">如果操作失败，将抛出的异常消息。</param>
        /// <exception cref="InvalidOperationException">当值不为null时抛出。</exception>
        /// <remarks>
        /// 如果值为null，则抛出<see cref="InvalidOperationException"/>异常，并附带指定的消息。
        /// </remarks>
        [DebuggerStepThrough]
        internal static void Operation<T>(this T value, string message) where T : class
        {
            if (value is not null) return;

            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// 根据条件执行操作。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="message">操作失败时抛出的异常信息。</param>
        /// <exception cref="InvalidOperationException">当 <paramref name="condition"/> 为 false 时抛出。</exception>
        [DebuggerStepThrough]
        internal static void Operation(this bool condition, string message)
        {
            if (condition) return;

            throw new InvalidOperationException(message);
        }

        #endregion
    }
}
