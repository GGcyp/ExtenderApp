using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.IO;
using ExtenderApp.Contracts;
using System.Runtime.CompilerServices;


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

        /// <summary>
        /// 抛出一个<see cref="ArgumentOutOfRangeException"/>异常，表示传入的参数超出了有效范围。
        /// </summary>
        /// <param name="parameterName">导致异常的参数名称。</param>
        /// <param name="message">异常的详细信息，可以为null。</param>
        /// <exception cref="ArgumentOutOfRangeException">当参数超出了有效范围时抛出。</exception>
        [DebuggerStepThrough]
        public static void ArgumentOutOfRange(string parameterName, string? message = null)
        {
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
        public static T ArgumentNull<T>([NotNull] this T value, [CallerMemberName] string parameterName = " ") where T : class
        {
            if (value != null) return value;

            throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// 检查传入的对象是否为null。
        /// 如果对象不为null，则直接返回该对象；如果为null，则抛出ArgumentNullException异常。
        /// </summary>
        /// <param name="value">要检查的对象。</param>
        /// <param name="parameterName">参数的名称。</param>
        /// <returns>如果不为null，则返回传入的对象。</returns>
        /// <exception cref="ArgumentNullException">当传入的对象为null时抛出。</exception>
        [DebuggerStepThrough]
        public static object ArgumentObjectNull([NotNull] this object value, string parameterName)
        {
            if (value != null) return value;

            throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// 抛出ArgumentNullException异常。
        /// </summary>
        /// <param name="parameterName">参数的名称。</param>
        /// <exception cref="ArgumentNullException">始终抛出ArgumentNullException异常。</exception>
        [DebuggerStepThrough]
        public static void ArgumentNull(string parameterName)
        {
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
        public static void Operation<T>(this T value, string message) where T : class
        {
            if (value is not null) return;

            //throw new InvalidOperationException(message);
            Operation(message);
        }

        /// <summary>
        /// 根据条件执行操作。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="message">操作失败时抛出的异常信息。</param>
        /// <exception cref="InvalidOperationException">当 <paramref name="condition"/> 为 false 时抛出。</exception>
        [DebuggerStepThrough]
        public static void Operation(this bool condition, string message)
        {
            if (condition) return;

            //throw new InvalidOperationException(message);
            Operation(message);
        }

        /// <summary>
        /// 当条件为假时，执行一个操作。
        /// </summary>
        /// <param name="condition">判断条件。</param>
        /// <param name="message">要传递的消息。</param>
        /// <param name="args">要传递的参数列表。</param>
        [DebuggerStepThrough]
        public static void Operation(this bool condition, string message, params object[] args)
        {
            if (condition) return;

            Operation(message, args);
        }

        /// <summary>
        /// 抛出一个异常，包含指定格式的消息和参数。
        /// </summary>
        /// <param name="message">包含占位符的格式化字符串。</param>
        /// <param name="args">与消息中的占位符对应的参数。</param>
        /// <exception cref="InvalidOperationException">如果调用此方法，则始终抛出此异常。</exception>
        [DebuggerStepThrough]
        public static void Operation(string message, params object[] args)
        {
            throw new InvalidOperationException(string.Format(message, args));
        }

        #endregion

        #region FileNotFoundException

        /// <summary>
        /// 抛出文件未找到的异常。
        /// </summary>
        /// <param name="message">异常消息。</param>
        /// <exception cref="FileNotFoundException">当文件未找到时抛出此异常。</exception>
        public static void FileNotFound(string message)
        {
            throw new FileNotFoundException(message);
        }

        /// <summary>
        /// 当文件未找到时抛出异常。
        /// </summary>
        /// <param name="folderPath">文件所在的文件夹路径。</param>
        /// <param name="fileName">文件名。</param>
        /// <exception cref="FileNotFoundException">当文件未找到时抛出。</exception>
        public static void FileNotFound(string folderPath, string fileName)
        {
            FileNotFound(string.Format("未发现文件，路径：{0}，名字：{1}", folderPath, fileName));
        }

        /// <summary>
        /// 文件未找到处理
        /// </summary>
        /// <param name="fileInfo">本地文件信息</param>
        /// <exception cref="ArgumentNullException">当文件路径为空时抛出</exception>
        public static void FileNotFound(this ExpectLocalFileInfo fileInfo)
        {
            if (fileInfo.IsEmpty)
                ArgumentNull("未知文件路径");

            FileNotFound(fileInfo.FolderPath, fileInfo.FileName);
        }

        /// <summary>
        /// 当文件未找到时抛出异常。
        /// </summary>
        /// <param name="fileInfo">本地文件信息对象。</param>
        /// <exception cref="ArgumentNullException">当文件路径为空时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件未找到时抛出。</exception>
        public static void FileNotFound(this LocalFileInfo fileInfo)
        {
            if (fileInfo.IsEmpty)
                ArgumentNull("未知文件路径");

            if (fileInfo.FileInfo.Exists)
                return;

            FileNotFound(fileInfo.FullPath);
        }

        #endregion
    }
}
