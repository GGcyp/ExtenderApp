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
        /// 使用一个数字初始化一个1x1的矩阵
        /// </summary>
        /// <param name="num">要初始化的矩阵中的唯一元素</param>
        public Matrix(double num) : this(1, 1)
        {
            _matrix[0, 0] = num;
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
                _matrix[i, 0] = doubles[i];
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

        #region Operator

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
                //判断是否为单一矩阵
                if (right.Row == 1 && right.Column == 1)
                {
                    return left + right[0, 0];
                }
                throw new ArgumentException("The dimensions of the matrices do not match, so addition cannot be performed.");
            }

            Matrix result = new Matrix(left.Row, left.Column);

            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] + right[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 重载加法运算符，用于矩阵与常数的加法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">常数。</param>
        /// <returns>返回矩阵与常数相加的结果。</returns>
        public static Matrix operator +(Matrix left, double @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] + @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 重载加法运算符，用于矩阵与整数的加法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">整数。</param>
        /// <returns>返回矩阵与整数相加的结果。</returns>
        public static Matrix operator +(Matrix left, int @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] + @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 重载加法运算符，将Matrix对象与double数组相加
        /// </summary>
        /// <param name="left">左操作数，Matrix类型</param>
        /// <param name="nums">右操作数，double数组</param>
        /// <returns>返回一个新的Matrix对象，其行数与left相同，列数比left多一列</returns>
        public static Matrix operator +(Matrix left, double[] nums)
        {
            if (nums.Length != left.Row)
                throw new ArgumentException("The length of nums array must match the number of rows in the Matrix.");

            Matrix result = new Matrix(left.Row, left.Column + 1);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j];
                }
                result[i, left.Column] = nums[i];
            }
            return result;
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

            Matrix result = new Matrix(left.Row, left.Column);

            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] - right[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 重载减法运算符，用于矩阵与常数的减法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">常数。</param>
        /// <returns>返回矩阵与常数相减的结果。</returns>
        public static Matrix operator -(Matrix left, double @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] - @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 重载减法运算符，用于矩阵与整数的减法运算。
        /// </summary>
        /// <param name="left">矩阵。</param>
        /// <param name="const">整数。</param>
        /// <returns>返回矩阵与整数相减的结果。</returns>
        public static Matrix operator -(Matrix left, int @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] - @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵与整数的乘法运算符重载。
        /// </summary>
        /// <param name="left">左侧操作数，矩阵类型。</param>
        /// <param name="const">右侧操作数，整型常量。</param>
        /// <returns>返回一个新的矩阵，其元素是原矩阵元素与整数的乘积。</returns>
        public static Matrix operator *(Matrix left, int @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] * @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵与双精度浮点数的乘法运算符重载。
        /// </summary>
        /// <param name="left">左侧操作数，矩阵类型。</param>
        /// <param name="const">右侧操作数，双精度浮点型常量。</param>
        /// <returns>返回一个新的矩阵，其元素是原矩阵元素与双精度浮点数的乘积。</returns>
        public static Matrix operator *(Matrix left, double @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] * @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵乘法运算符重载
        /// </summary>
        /// <param name="left">左矩阵</param>
        /// <param name="right">右矩阵</param>
        /// <returns>返回两个矩阵相乘的结果</returns>
        /// <exception cref="ArgumentException">如果左矩阵的列数不等于右矩阵的行数，则抛出异常</exception>
        public static Matrix operator *(Matrix left, Matrix right)
        {
            if (left.Column != right.Row)
            {
                if (right.Column != 1 && right.Row != 1)
                    //左矩阵的列数必须等于右矩阵的行数才能进行乘法运算。
                    throw new ArgumentException("The number of columns in the first matrix must be equal to the number of rows in the second matrix for multiplication to be possible.");

                return left * right[0, 0];
            }

            Matrix result = new Matrix(left.Row, right.Column);

            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < right.Column; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < left.Column; k++)
                    {
                        sum += left[i, k] * right[k, j];
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵与整数的除法运算符重载。
        /// </summary>
        /// <param name="left">左侧操作数，矩阵类型。</param>
        /// <param name="const">右侧操作数，整型常量。</param>
        /// <returns>返回一个新的矩阵，其元素是原矩阵元素与整数的商。</returns>
        public static Matrix operator /(Matrix left, int @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] / @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵与双精度浮点数的除法运算符重载。
        /// </summary>
        /// <param name="left">左侧操作数，矩阵类型。</param>
        /// <param name="const">右侧操作数，双精度浮点型常量。</param>
        /// <returns>返回一个新的矩阵，其元素是原矩阵元素与双精度浮点数的商。</returns>
        public static Matrix operator /(Matrix left, double @const)
        {
            Matrix result = new Matrix(left.Row, left.Column);
            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < left.Column; j++)
                {
                    result[i, j] = left[i, j] / @const;
                }
            }
            return result;
        }

        /// <summary>
        /// 矩阵除法运算符重载
        /// </summary>
        /// <param name="left">左矩阵</param>
        /// <param name="right">右矩阵</param>
        /// <returns>返回两个矩阵相除的结果</returns>
        /// <exception cref="ArgumentException">如果左矩阵的列数不等于右矩阵的行数，则抛出异常</exception>
        public static Matrix operator /(Matrix left, Matrix right)
        {
            if (left.Column != right.Row)
            {
                if (right.Column != 1 && right.Row != 1)
                    //左矩阵的列数必须等于右矩阵的行数才能进行除法运算。
                    throw new ArgumentException("The number of columns in the first matrix must be equal to the number of rows in the second matrix for division to be possible.");


                return left / right[0, 0];
            }

            Matrix result = new Matrix(left.Row, right.Column);

            for (int i = 0; i < left.Row; i++)
            {
                for (int j = 0; j < right.Column; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < left.Column; k++)
                    {
                        sum += left[i, k] * right[k, j];
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }

        public static Matrix operator ^(Matrix left, int n)
        {
            //负整数次幂未实现

            Matrix result = new Matrix(left.Row, left.Column);

            if (n == 0)
            {
                int count = Math.Min(left.Row, left.Column);
                for (int i = 0; i < count; i++)
                {
                    result[i, i] = 0;
                }
                return result;
            }

            for(int index=0; index < n; index++)
            {
                for (int i = 0; i < left.Row; i++)
                {
                    for (int j = 0; j < left.Column; j++)
                    {
                        result[i, j] = left[i, j] * left[i, j];
                    }
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 创建一个 n*n 的单位矩阵
        /// </summary>
        /// <param name="n">矩阵的维数</param>
        /// <param name="num">矩阵中元素的值</param>
        /// <returns>返回一个 n*n 的单位矩阵</returns>
        public static Matrix Identity(int n, double num = 1)
        {
            Matrix matrix = new(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = num;
                }
            }

            return matrix;
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
