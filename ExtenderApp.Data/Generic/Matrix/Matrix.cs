using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 矩阵结构体。
    /// </summary>
    public struct Matrix
    {
        /// <summary>
        /// 获取矩阵的行数。
        /// </summary>
        public int Row { get; }
        /// <summary>
        /// 获取矩阵的列数。
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// 判断矩阵是否为方阵（行数和列数相等）
        /// </summary>
        /// <returns>如果矩阵是方阵则返回true，否则返回false</returns>
        public bool IsSquareMatrix => Row == Column;

        /// <summary>
        /// 是否为空矩阵
        /// </summary>
        public bool IsEmpty => _matrix is null;

        /// <summary>
        /// 存储矩阵的二维数组。
        /// </summary>
        private readonly double[,] _matrix;

        /// <summary>
        /// 获取或设置矩阵中指定位置的元素。
        /// </summary>
        /// <param name="row">行索引。</param>
        /// <param name="col">列索引。</param>
        /// <returns>矩阵中指定位置的元素。</returns>
        public double this[int row, int col]
        {
            get => _matrix[row, col];
            set => _matrix[row, col] = value;
        }

        /// <summary>
        /// 使用指定的维度 n 初始化 Matrix 结构的新实例，矩阵为方阵。
        /// </summary>
        /// <param name="n">矩阵的维度（行数和列数相等）。</param>
        public Matrix(int n) : this(n, n)
        {
        }

        /// <summary>
        /// 使用指定的行数和列数初始化 Matrix 结构的新实例。
        /// </summary>
        /// <param name="row">矩阵的行数。</param>
        /// <param name="column">矩阵的列数。</param>
        public Matrix(int row, int column)
        {
            if (row < 1 || column < 1)
                throw new ArgumentNullException(nameof(row) + ":" + nameof(column));

            Row = row;
            Column = column;
            _matrix = new double[row, column];
        }

        /// <summary>
        /// 使用给定的double数组初始化一个矩阵对象。
        /// </summary>
        /// <param name="doubles">一个包含矩阵元素的double数组。</param>
        public Matrix(double[] doubles)
        {
            Row = doubles.Length;
            Column = 1;

            _matrix = new double[Row, Column];
            for (int i = 0; i < Row; i++)
            {
                _matrix[i, Column] = doubles[i];
            }
        }

        /// <summary>
        /// 初始化Matrix对象
        /// </summary>
        /// <param name="doubles">一个二维List<List<double>>，用于初始化矩阵</param>
        public Matrix(List<List<double>> doubles)
        {
            Row = doubles.Count;
            Column = doubles[0].Count;
            _matrix = new double[Row, Column];
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Column; j++)
                {
                    _matrix[i, j] = doubles[i][j];
                }
            }
        }

        /// <summary>
        /// 使用一个二维数组初始化 Matrix 结构的新实例。
        /// </summary>
        /// <param name="matrix">初始化矩阵的二维数组。</param>
        public Matrix(double[,] matrix)
        {
            this._matrix = matrix;
            Row = matrix.GetLength(0);
            Column = matrix.GetLength(1);
        }

        /// <summary>
        /// 克隆当前矩阵对象。
        /// </summary>
        /// <returns>返回当前矩阵对象的深拷贝。</returns>
        public Matrix Clone()
        {
            return new Matrix((double[,])_matrix.Clone());
        }

        /// <summary>
        /// 重载加法运算符，用于矩阵加法运算。
        /// </summary>
        /// <param name="left">左侧矩阵。</param>
        /// <param name="right">右侧矩阵。</param>
        /// <returns>返回两个矩阵相加的结果。</returns>
        /// <exception cref="ArgumentException">当两个矩阵的维度不匹配时抛出。</exception>
        public static Matrix operator +(Matrix left, Matrix right)
        {
            if (left.Row != right.Row || left.Column != right.Column)
            {
                throw new ArgumentException("The dimensions of the matrices do not match, so addition cannot be performed.");
            }

            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    left[i, j] += right[i, j];
                }
            }
            return left;
        }

        /// <summary>
        /// 重载加法运算符，用于矩阵与常数的加法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">常数。</param>
        /// <returns>返回矩阵与常数相加的结果。</returns>
        public static Matrix operator +(Matrix left, double @const)
        {
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    left[i, j] += @const;
                }
            }
            return left;
        }

        /// <summary>
        /// 重载加法运算符，用于矩阵与整数的加法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">整数。</param>
        /// <returns>返回矩阵与整数相加的结果。</returns>
        public static Matrix operator +(Matrix left, int @const)
        {
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    left[i, j] += @const;
                }
            }
            return left;
        }

        /// <summary>
        /// 重载减法运算符，用于矩阵减法运算。
        /// </summary>
        /// <param name="left">左侧矩阵。</param>
        /// <param name="right">右侧矩阵。</param>
        /// <returns>返回两个矩阵相减的结果。</returns>
        /// <exception cref="ArgumentException">当两个矩阵的维度不匹配时抛出。</exception>
        public static Matrix operator -(Matrix left, Matrix right)
        {
            if (left.Row != right.Row || left.Column != right.Column)
            {
                throw new ArgumentException("The dimensions of the matrices do not match, so addition cannot be performed.");
            }

            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    left[i, j] -= right[i, j];
                }
            }
            return left;
        }

        /// <summary>
        /// 重载减法运算符，用于矩阵与常数的减法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">常数。</param>
        /// <returns>返回矩阵与常数相减的结果。</returns>
        public static Matrix operator -(Matrix left, double @const)
        {
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    left[i, j] -= @const;
                }
            }
            return left;
        }

        /// <summary>
        /// 重载减法运算符，用于矩阵与整数的减法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">整数。</param>
        /// <returns>返回矩阵与整数相减的结果。</returns>
        public static Matrix operator -(Matrix left, int @const)
        {
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    left[i, j] -= @const;
                }
            }
            return left;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Row; i++)
            {
                sb.Append("[");
                for (int j = 0; j < Column; j++)
                {
                    sb.Append(_matrix[i, j]);
                    if (Column - 1 > 0) sb.Append(", ");
                }
                sb.Append("]");
                sb.Append("\t");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
