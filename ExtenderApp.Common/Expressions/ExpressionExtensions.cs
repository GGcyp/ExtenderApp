using System.Linq.Expressions;
using System.Reflection;

namespace ExtenderApp.Common.Expressions
{
    /// <summary>
    /// 基于表达式树与委托的反射辅助扩展。
    /// 提供从表达式或委托中安全地提取 <see cref="MethodInfo"/>、<see cref="PropertyInfo"/>、<see cref="FieldInfo"/> 与 <see cref="MemberInfo"/> 的方法。
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// 从方法调用表达式中获取 <see cref="MethodInfo"/>。
        /// </summary>
        /// <typeparam name="T">表达式中 <c>this</c> 的目标类型。</typeparam>
        /// <typeparam name="TResult">表达式返回类型，需为 <see cref="Delegate"/>。</typeparam>
        /// <param name="item">扩展方法的目标实例，仅用于类型绑定。</param>
        /// <param name="expression">方法调用的表达式，如 <c>x =&gt; x.SomeMethod(...)</c>。</param>
        /// <returns>被调用方法的 <see cref="MethodInfo"/>。</returns>
        /// <exception cref="ArgumentException">当表达式不是方法调用时抛出。</exception>
        public static MethodInfo GetMethodInfo<T, TResult>(this T item, Expression<Func<T, TResult>> expression)
            where TResult : Delegate
        {
            if (expression.Body is MethodCallExpression methodCall)
                return methodCall.Method;

            throw new ArgumentException(nameof(expression));
        }

        /// <summary>
        /// 从委托实例中获取 <see cref="MethodInfo"/>。
        /// </summary>
        /// <typeparam name="T">扩展方法的目标类型。</typeparam>
        /// <typeparam name="TDelegate">委托类型。</typeparam>
        /// <param name="item">扩展方法的目标实例，仅用于类型绑定。</param>
        /// <param name="delegate">委托实例。</param>
        /// <returns>委托所指向方法的 <see cref="MethodInfo"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="delegate"/> 为 null 时抛出。</exception>
        public static MethodInfo GetMethodInfo<T, TDelegate>(this T item, TDelegate @delegate)
            where TDelegate : Delegate
        {
            if (@delegate != null)
                return @delegate.Method;

            throw new ArgumentNullException(nameof(@delegate));
        }

        /// <summary>
        /// 从成员访问表达式中获取 <see cref="PropertyInfo"/>。
        /// </summary>
        /// <typeparam name="T">表达式中 <c>this</c> 的目标类型。</typeparam>
        /// <typeparam name="TResult">属性的返回类型。</typeparam>
        /// <param name="item">扩展方法的目标实例，仅用于类型绑定。</param>
        /// <param name="expression">属性访问表达式，如 <c>x =&gt; x.Property</c>。</param>
        /// <returns>被访问属性的 <see cref="PropertyInfo"/>。</returns>
        /// <exception cref="ArgumentException">当表达式不是属性访问时抛出。</exception>
        public static PropertyInfo GetPropertyInfo<T, TResult>(this T item, Expression<Func<T, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
                return propertyInfo;

            throw new ArgumentException("需要传入属性", nameof(expression));
        }

        /// <summary>
        /// 从成员访问表达式中获取 <see cref="FieldInfo"/>。
        /// </summary>
        /// <typeparam name="T">表达式中 <c>this</c> 的目标类型。</typeparam>
        /// <typeparam name="TResult">字段的返回类型。</typeparam>
        /// <param name="item">扩展方法的目标实例，仅用于类型绑定。</param>
        /// <param name="expression">字段访问表达式，如 <c>x =&gt; x.Field</c>。</param>
        /// <returns>被访问字段的 <see cref="FieldInfo"/>。</returns>
        /// <exception cref="ArgumentException">当表达式不是字段访问时抛出。</exception>
        public static FieldInfo GetFieldInfo<T, TResult>(this T item, Expression<Func<T, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is FieldInfo fieldInfo)
                return fieldInfo;

            throw new ArgumentException("需要传入字段", nameof(expression));
        }

        /// <summary>
        /// 从成员访问表达式中获取 <see cref="MemberInfo"/>。
        /// </summary>
        /// <typeparam name="T">表达式中 <c>this</c> 的目标类型。</typeparam>
        /// <typeparam name="TResult">成员的返回类型。</typeparam>
        /// <param name="item">扩展方法的目标实例，仅用于类型绑定。</param>
        /// <param name="expression">成员访问表达式，如 <c>x =&gt; x.Member</c>。</param>
        /// <returns>被访问成员的 <see cref="MemberInfo"/>。</returns>
        /// <exception cref="ArgumentException">当表达式不是成员访问时抛出。</exception>
        public static MemberInfo GetMemberInfo<T, TResult>(this T item, Expression<Func<T, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Type;

            throw new ArgumentException("需要传入成员", nameof(expression));
        }
    }
}
