

using System.Diagnostics.CodeAnalysis;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示矩形边界的厚度，用于定义元素周围或内部的间距（上、右、下、左）
    /// </summary>
    /// <remarks>
    /// 1. 每个属性值以像素为单位，1像素 = 1/96英寸
    /// 2. 默认值为0，表示无间距
    /// 3. 类似CSS中的margin/padding概念
    /// </remarks>
    public struct ExThickness : IEquatable<ExThickness>
    {
        /// <summary>
        /// 获取或设置左边界的宽度（像素）
        /// </summary>
        /// <value>
        /// 双精度浮点数，表示左边界宽度，默认值0
        /// </value>
        public double Left { get; set; }

        /// <summary>
        /// 获取或设置上边界的宽度（像素）
        /// </summary>
        /// <value>
        /// 双精度浮点数，表示上边界宽度，默认值0
        /// </value>
        public double Top { get; set; }

        /// <summary>
        /// 获取或设置右边界的宽度（像素）
        /// </summary>
        /// <value>
        /// 双精度浮点数，表示右边界宽度，默认值0
        /// </value>
        public double Right { get; set; }

        /// <summary>
        /// 获取或设置下边界的宽度（像素）
        /// </summary>
        /// <value>
        /// 双精度浮点数，表示下边界宽度，默认值0
        /// </value>
        public double Bottom { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uniformLength">统一设置四个方向的厚度</param>
        public ExThickness(double uniformLength)
        {
            Left = Right = Top = Bottom = uniformLength;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="left">左边界宽度</param>
        /// <param name="top">上边界宽度</param>
        /// <param name="right">右边界宽度</param>
        /// <param name="bottom">下边界宽度</param>
        public ExThickness(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not ExThickness thickness)
                return false;

            return Equals(thickness);
        }
        public bool Equals(ExThickness thickness)
        {
            return Left == thickness.Left &&
                Top == thickness.Top &&
                Right == thickness.Right &&
                Bottom == thickness.Bottom;
        }

        public override string ToString()
        {
            return $"{Left}-{Top}-{Right}-{Bottom}";
        }

        public static bool operator ==(ExThickness left, ExThickness right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ExThickness left, ExThickness right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = default;

            hashCode.Add(Left);
            hashCode.Add(Top);
            hashCode.Add(Right);
            hashCode.Add(Bottom);

            return hashCode.ToHashCode();
        }
    }
}
