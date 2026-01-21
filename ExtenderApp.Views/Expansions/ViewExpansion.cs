using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 提供视图相关枚举和结构体的扩展转换方法
    /// </summary>
    public static class ViewExpansion
    {
        /// <summary>
        /// 将自定义水平对齐枚举转换为系统水平对齐枚举
        /// </summary>
        /// <param name="ex">自定义水平对齐枚举值</param>
        /// <returns>对应的系统水平对齐枚举值</returns>
        public static HorizontalAlignment ToHorizontalAlignment(this ExHorizontalAlignment ex)
        {
            return ex switch
            {
                ExHorizontalAlignment.Right => HorizontalAlignment.Right,
                ExHorizontalAlignment.Stretch => HorizontalAlignment.Stretch,
                ExHorizontalAlignment.Left => HorizontalAlignment.Left,
                ExHorizontalAlignment.Center => HorizontalAlignment.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(ex), ex, null)
            };
        }

        /// <summary>
        /// 将系统水平对齐枚举转换为自定义水平对齐枚举
        /// </summary>
        /// <param name="h">系统水平对齐枚举值</param>
        /// <returns>对应的自定义水平对齐枚举值</returns>
        public static ExHorizontalAlignment ToExHorizontalAlignment(this HorizontalAlignment h)
        {
            return h switch
            {
                HorizontalAlignment.Right => ExHorizontalAlignment.Right,
                HorizontalAlignment.Stretch => ExHorizontalAlignment.Stretch,
                HorizontalAlignment.Left => ExHorizontalAlignment.Left,
                HorizontalAlignment.Center => ExHorizontalAlignment.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(h), h, null)
            };
        }

        /// <summary>
        /// 将自定义垂直对齐枚举转换为系统垂直对齐枚举
        /// </summary>
        /// <param name="ex">自定义垂直对齐枚举值</param>
        /// <returns>对应的系统垂直对齐枚举值</returns>
        public static VerticalAlignment ToVerticalAlignment(this ExVerticalAlignment ex)
        {
            return ex switch
            {
                ExVerticalAlignment.Top => VerticalAlignment.Top,
                ExVerticalAlignment.Center => VerticalAlignment.Center,
                ExVerticalAlignment.Bottom => VerticalAlignment.Bottom,
                ExVerticalAlignment.Stretch => VerticalAlignment.Stretch,
                _ => throw new ArgumentOutOfRangeException(nameof(ex), ex, null)
            };
        }

        /// <summary>
        /// 将系统垂直对齐枚举转换为自定义垂直对齐枚举
        /// </summary>
        /// <param name="v">系统垂直对齐枚举值</param>
        /// <returns>对应的自定义垂直对齐枚举值</returns>
        public static ExVerticalAlignment ToExVerticalAlignment(this VerticalAlignment v)
        {
            return v switch
            {
                VerticalAlignment.Top => ExVerticalAlignment.Top,
                VerticalAlignment.Center => ExVerticalAlignment.Center,
                VerticalAlignment.Bottom => ExVerticalAlignment.Bottom,
                VerticalAlignment.Stretch => ExVerticalAlignment.Stretch,
                _ => throw new ArgumentOutOfRangeException(nameof(v), v, null)
            };
        }

        /// <summary>
        /// 将自定义厚度结构转换为系统厚度结构
        /// </summary>
        /// <param name="ex">自定义厚度结构实例</param>
        /// <returns>对应的系统厚度结构实例</returns>
        public static Thickness ToThickness(this ExThickness ex)
        {
            return new Thickness(ex.Left, ex.Top, ex.Right, ex.Bottom);
        }

        /// <summary>
        /// 将系统厚度结构转换为自定义厚度结构
        /// </summary>
        /// <param name="t">系统厚度结构实例</param>
        /// <returns>对应的自定义厚度结构实例</returns>
        public static ExThickness ToExThickness(this Thickness t)
        {
            return new ExThickness(t.Left, t.Top, t.Right, t.Bottom);
        }
    }
}