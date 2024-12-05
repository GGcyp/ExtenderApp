using ExtenderApp.Data;

namespace ExtenderApp.Common.Math
{
    /// <summary>
    /// 矩阵拓展类
    /// </summary>
    public static class MatrixExtensions
    {
        #region 基础矩阵运算

        /// <summary>
        /// 将两个矩阵相加。
        /// </summary>
        /// <param name="matrixLeft">左侧矩阵。</param>
        /// <param name="matrixRight">右侧矩阵。</param>
        /// <returns>返回相加后的左矩阵。</returns>
        /// <exception cref="ArgumentException">如果两个矩阵的维度不匹配且右侧矩阵不是1x1矩阵，则抛出异常。</exception>
        public static Matrix Sum(this Matrix matrixLeft, Matrix matrixRight)
        {
            // 首先判断两个矩阵的维度（行数和列数）是否一致，如果不一致则进一步判断是否是右矩阵为1x1的特殊情况（可当作标量来处理与左矩阵相加）
            if (matrixLeft.Row != matrixRight.Row || matrixLeft.Column != matrixRight.Column)
            {
                if (matrixRight.Row == 1 && matrixRight.Column == 1)
                {
                    for (int i = 0; i < matrixLeft.Row; i++)
                    {
                        for (int j = 0; j < matrixRight.Column; j++)
                        {
                            matrixLeft[i, j] += matrixRight[1, 1];
                        }
                    }
                    return matrixLeft;
                }
                throw new ArgumentException("The dimensions of the matrices do not match, so addition cannot be performed.");
            }

            for (int i = 0; i < matrixLeft.Row; i++)
            {
                for (int j = 0; j < matrixLeft.Column; j++)
                {
                    matrixLeft[i, j] += matrixRight[i, j];
                }
            }

            return matrixLeft;
        }

        /// <summary>
        /// 将两个矩阵相减。
        /// </summary>
        /// <param name="matrixLeft">左侧矩阵。</param>
        /// <param name="matrixRight">右侧矩阵。</param>
        /// <returns>返回相减后的左矩阵。</returns>
        /// <exception cref="ArgumentException">如果两个矩阵的维度不匹配且右侧矩阵不是1x1矩阵，则抛出异常。</exception>
        public static Matrix Sub(this Matrix matrixLeft, Matrix matrixRight)
        {
            // 首先判断两个矩阵的维度（行数和列数）是否一致，如果不一致则进一步判断是否是右矩阵为1x1的特殊情况（可当作标量来处理与左矩阵相加）
            if (matrixLeft.Row != matrixRight.Row || matrixLeft.Column != matrixRight.Column)
            {
                if (matrixRight.Row == 1 && matrixRight.Column == 1)
                {
                    for (int i = 0; i < matrixLeft.Row; i++)
                    {
                        for (int j = 0; j < matrixRight.Column; j++)
                        {
                            matrixLeft[i, j] -= matrixRight[1, 1];
                        }
                    }
                    return matrixLeft;
                }
                throw new ArgumentException("The dimensions of the matrices do not match, so addition cannot be performed.");
            }

            for (int i = 0; i < matrixLeft.Row; i++)
            {
                for (int j = 0; j < matrixLeft.Column; j++)
                {
                    matrixLeft[i, j] -= matrixRight[i, j];
                }
            }

            return matrixLeft;
        }

        /// <summary>
        /// 矩阵乘法扩展方法
        /// </summary>
        /// <param name="matrixLeft">左矩阵</param>
        /// <param name="matrixRight">右矩阵</param>
        /// <param name="result">结果矩阵，默认为空矩阵</param>
        /// <returns>返回乘法结果矩阵</returns>
        /// <exception cref="ArgumentException">当左矩阵的列数不等于右矩阵的行数时抛出</exception>
        public static Matrix Multiplication(this Matrix matrixLeft, Matrix matrixRight, Matrix result = default)
        {
            // 判断左矩阵的列数是否等于右矩阵的行数，若不相等则抛出参数异常，因为不符合矩阵乘法的基本规则，无法进行乘法运算
            if (matrixLeft.Column != matrixRight.Row)
            {
                throw new ArgumentException("The number of columns in the first matrix must be equal to the number of rows in the second matrix for multiplication to be possible.");
            }

            if (result.IsEmpty || result.Row != matrixLeft.Row || result.Column != matrixRight.Column)
                result = new Matrix(matrixLeft.Row, matrixRight.Column);


            for (int i = 0; i < matrixLeft.Row; i++)
            {
                for (int j = 0; j < matrixRight.Column; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < matrixLeft.Column; k++)
                    {
                        sum += matrixLeft[i, k] * matrixRight[k, j];
                    }
                    result[i, j] = sum;
                }
            }

            return result;
        }

        /// <summary>
        /// 矩阵数乘扩展方法
        /// </summary>
        /// <param name="matrixLeft">待数乘的矩阵</param>
        /// <param name="num">乘数</param>
        /// <returns>返回数乘后的矩阵,返回矩阵为修改后的待数乘矩阵</returns>

        public static Matrix Multiplication(this Matrix matrixLeft, double num)
        {
            for (int i = 0; i < matrixLeft.Row; i++)
            {
                for (int j = 0; j < matrixLeft.Column; j++)
                {
                    matrixLeft[i, j] *= num;
                }
            }

            return matrixLeft;
        }

        #endregion

        #region 矩阵运算

        /// <summary>
        /// 复制矩阵行数和列数，如果目标矩阵不为空但行列不一致则报错
        /// </summary>
        /// <param name="matrix">源矩阵</param>
        /// <param name="targetMatrix">目标矩阵，默认为null</param>
        /// <returns>返回目标矩阵</returns>
        /// <exception cref="ArgumentException">如果目标矩阵和原矩阵行列不相等，则抛出此异常</exception>
        public static Matrix CopyTo(this Matrix matrix, Matrix targetMatrix = default)
        {
            if (targetMatrix.IsEmpty)
            {
                targetMatrix = new Matrix(matrix.Row, matrix.Column);
            }
            else
            {
                if (targetMatrix.Row < matrix.Row || targetMatrix.Column < matrix.Column)
                    throw new ArgumentException("The target matrix and the original matrix have different row and column dimensions.");

                for (int i = 0; i < matrix.Row; i++)
                {
                    for (int j = 0; j < matrix.Column; j++)
                    {
                        targetMatrix[i, j] = matrix[i, j];
                    }
                }
            }
            return targetMatrix;
        }

        /// <summary>
        /// 计算矩阵的行列式。
        /// </summary>
        /// <param name="matrix">要计算行列式的矩阵。</param>
        /// <param name="luMatrix">LU分解后的矩阵，默认为空。如果提供，将使用该矩阵进行计算。</param>
        /// <returns>返回计算后的LU分解矩阵。</returns>
        /// <exception cref="ArgumentNullException">如果输入的矩阵为空，则抛出此异常。</exception>
        /// <exception cref="ArgumentException">如果输入的矩阵不是方阵，则抛出此异常。</exception>
        public static Matrix CalculateDeterminant(this Matrix matrix, Matrix luMatrix = default)
        {
            if (matrix.IsEmpty)
                throw new ArgumentNullException(nameof(matrix));

            if (!matrix.IsSquareMatrix)
                throw new ArgumentException("Matrix must be square.");

            int n = matrix.Row;
            //矩阵的长度
            luMatrix = matrix.CopyTo(luMatrix);


            //原点
            luMatrix[0, 0] = matrix[0, 0];

            //a12,a21,a22
            luMatrix[0, 1] = matrix[0, 1];
            luMatrix[1, 0] = matrix[1, 0] / luMatrix[0, 0];
            luMatrix[1, 1] = matrix[1, 1] - luMatrix[1, 0] * luMatrix[0, 1];

            //从第二开始
            for (int i = 2; i < n; i++)
            {
                //生成行和列第一数
                luMatrix[0, i] = matrix[0, i];
                luMatrix[i, 0] = matrix[i, 0] / luMatrix[0, 0];

                for (int j = 1; j < i; j++)
                {
                    //上三角部分
                    luMatrix[j, i] = matrix[j, i] - luMatrix.LUCalculation(i, j, j);

                    // 下三角部分
                    luMatrix[i, j] = (matrix[i, j] - luMatrix.LUCalculation(j, i, j)) / luMatrix[j, j];
                }

                //最后的上三角原点
                luMatrix[i, i] = matrix[i, i] - luMatrix.LUCalculation(i, i, i);
            }

            return luMatrix;
        }

        /// <summary>
        /// 计算LU矩阵的特定子矩阵的LU分解结果。
        /// </summary>
        /// <param name="luMatrix">待计算的LU矩阵。</param>
        /// <param name="xIndex">子矩阵的起始行索引。</param>
        /// <param name="yIndex">子矩阵的起始列索引。</param>
        /// <param name="count">子矩阵的大小。</param>
        /// <returns>LU分解后的结果。</returns>
        private static double LUCalculation(this Matrix luMatrix, int xIndex, int yIndex, int count)
        {
            double reslut = 0.0;
            for (int i = 0; i < count; i++)
            {
                reslut += luMatrix[i, xIndex] * luMatrix[yIndex, i];
            }
            return reslut;
        }

        /// <summary>
        /// 将矩阵转置。
        /// </summary>
        /// <param name="matrix">要转置的矩阵。</param>
        /// <param name="transpose">可选参数，用于存储转置后的矩阵。如果为null或默认值，将创建一个新的矩阵来存储结果。</param>
        /// <returns>转置后的矩阵。</returns>
        /// <exception cref="ArgumentNullException">如果输入的矩阵为空。</exception>
        /// <remarks>
        /// 此方法将输入的矩阵转置，即行和列互换。如果输入的矩阵为空，则抛出异常。
        /// </remarks>
        public static Matrix Transpose(this Matrix matrix, Matrix transpose = default)
        {
            if (matrix.IsEmpty)
                throw new ArgumentNullException(nameof(matrix));

            //if (!IsSquareMatrix(matrix))
            //    throw new ArgumentException("Matrix must be square.");

            int row = matrix.Column;
            int col = matrix.Row;
            if (transpose.IsEmpty || (transpose.Row != row || transpose.Column != col))
                transpose = new Matrix(row, col);


            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    transpose[j, i] = matrix[i, j];
                }
            }

            return transpose;
        }

        /// <summary>
        /// 执行矩阵乘法运算。
        /// </summary>
        /// <param name="matrixLeft">左侧矩阵。</param>
        /// <param name="matrixRight">右侧矩阵。</param>
        /// <param name="resultMatrix">可选参数，结果矩阵。如果传入的是空矩阵或不符合要求的矩阵，则会自动创建一个新的结果矩阵。适合重复使用时内存优化</param>
        /// <returns>返回结果矩阵。</returns>
        /// <exception cref="ArgumentNullException">如果左侧矩阵或右侧矩阵为空，则抛出此异常。</exception>
        /// <exception cref="ArgumentException">如果左侧矩阵的列数不等于右侧矩阵的行数，则抛出此异常。</exception>
        public static Matrix Dot(this Matrix matrixLeft, Matrix matrixRight, Matrix resultMatrix = default)
        {
            // 检查输入矩阵是否为空
            if (matrixLeft.IsEmpty)
                throw new ArgumentNullException(nameof(matrixLeft));
            if (matrixRight.IsEmpty)
                throw new ArgumentNullException(nameof(matrixRight));

            //// 检查矩阵是否为同型矩阵，满足点乘条件
            //if (matrixLeft.Row != matrixRight.Row || matrixLeft.Column != matrixRight.Column)
            //{
            //    if (!canKroneckerProduct)
            //        throw new ArgumentException("进行矩阵点乘的两个矩阵必须是同型矩阵。");

            //    matrixRight = matrixLeft.KroneckerProduct(matrixRight);
            //}

            if (matrixLeft.Column != matrixRight.Row)
                throw new ArgumentException("左侧矩阵的列数必须等于右侧矩阵的行数才能进行矩阵乘法。");

            //// 创建结果矩阵
            //Matrix resultMatrix = new Matrix(matrixLeft.Row, matrixLeft.Column);

            //// 执行矩阵点乘运算
            //for (int i = 0; i < matrixLeft.Row; i++)
            //{
            //    for (int j = 0; j < matrixLeft.Column; j++)
            //    {
            //        resultMatrix[i, j] = matrixLeft[i, j] * matrixRight[i, j];
            //    }
            //}

            if (resultMatrix.IsEmpty || resultMatrix.Row != matrixLeft.Row || resultMatrix.Column != matrixRight.Column)
                resultMatrix = new Matrix(matrixLeft.Row, matrixRight.Column);

            for (int i = 0; i < matrixLeft.Row; i++)
            {
                for (int j = 0; j < matrixRight.Column; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < matrixLeft.Column; k++)
                    {
                        sum += matrixLeft[i, k] * matrixRight[k, j];
                    }
                    resultMatrix[i, j] = sum;
                }
            }

            return resultMatrix;
        }

        /// <summary>
        /// 交换矩阵的两行
        /// </summary>
        /// <param name="matrix">要进行行交换的矩阵</param>
        /// <param name="row1">要交换的第一行的索引</param>
        /// <param name="row2">要交换的第二行的索引</param>
        /// <param name="n">矩阵的列数</param>
        public static void SwapRows(this Matrix matrix, int row1, int row2, int n)
        {
            for (int i = 0; i < n; i++)
            {
                double temp = matrix[row1, i];
                matrix[row1, i] = matrix[row2, i];
                matrix[row2, i] = temp;
            }
        }

        /// <summary>
        /// 将矩阵的某一行乘以一个标量
        /// </summary>
        /// <param name="matrix">要进行行乘法的矩阵</param>
        /// <param name="row">要乘以标量的行的索引</param>
        /// <param name="scalar">要乘以的标量</param>
        /// <param name="n">矩阵的列数</param>
        public static void MultiplyRow(this Matrix matrix, int row, double scalar, int n)
        {
            for (int i = 0; i < n; i++)
            {
                matrix[row, i] *= scalar;
            }
        }

        /// <summary>
        /// 将矩阵的一行加上另一行的倍数
        /// </summary>
        /// <param name="matrix">要进行行加法的矩阵</param>
        /// <param name="targetRow">目标行的索引</param>
        /// <param name="sourceRow">源行的索引</param>
        /// <param name="multiple">源行需要乘以的倍数</param>
        /// <param name="n">矩阵的列数</param>
        public static void AddRows(this Matrix matrix, int targetRow, int sourceRow, double multiple, int n)
        {
            for (int i = 0; i < n; i++)
            {
                matrix[targetRow, i] += multiple * matrix[sourceRow, i];
            }
        }

        /// <summary>
        /// 计算矩阵的行列式值
        /// </summary>
        /// <param name="matrix">待计算的矩阵</param>
        /// <param name="n">矩阵的维度</param>
        /// <returns>矩阵的行列式值</returns>
        public static double CalculateDeterminant(this Matrix matrix, int n)
        {
            Matrix workingMatrix = matrix.Clone();
            double determinant = 1;

            // 高斯消元化为上三角矩阵
            for (int i = 0; i < n; i++)
            {
                // 寻找当前列绝对值最大的元素所在行
                int maxRowIndex = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (System.Math.Abs(workingMatrix[k, i]) > System.Math.Abs(workingMatrix[maxRowIndex, i]))
                    {
                        maxRowIndex = k;
                    }
                }

                // 如果主对角线元素为0且当前列没有非零元素可替换，行列式为0
                if (workingMatrix[maxRowIndex, i] == 0)
                {
                    return 0;
                }

                // 交换当前行与绝对值最大元素所在行（若需要）
                if (maxRowIndex != i)
                {
                    SwapRows(workingMatrix, i, maxRowIndex, n);
                    determinant *= -1;
                }

                // 将当前行主对角线元素化为1
                double pivot = workingMatrix[i, i];
                determinant *= pivot;
                MultiplyRow(workingMatrix, i, 1 / pivot, n);

                // 用当前行将下面各行的当前列元素化为0
                for (int j = i + 1; j < n; j++)
                {
                    double factor = workingMatrix[j, i];
                    AddRows(workingMatrix, j, i, -factor, n);
                }
            }

            // 上三角矩阵的行列式等于主对角线元素之积
            for (int i = 0; i < n; i++)
            {
                determinant *= workingMatrix[i, i];
            }

            return determinant;
        }

        /// <summary>
        /// 判断一个矩阵是否为方阵且可逆。
        /// </summary>
        /// <param name="matrix">输入的矩阵。</param>
        /// <returns>如果矩阵是方阵且可逆，则返回true；否则返回false。</returns>
        public static bool IsSquareAndInvertible(this Matrix matrix)
        {
            if (!matrix.IsSquareMatrix)
            {
                return false;
            }

            double determinant = CalculateDeterminant(matrix, matrix.Row);
            return determinant != 0;
        }

        /// <summary>
        /// 计算矩阵的逆矩阵。
        /// </summary>
        /// <param name="matrix">输入的矩阵。</param>
        /// <param name="augmentedMatrix">增广矩阵，默认值为默认矩阵。</param>
        /// <param name="inverseMatrix">逆矩阵，默认值为默认矩阵。</param>
        /// <returns>计算得到的逆矩阵。</returns>
        /// <exception cref="ArgumentException">如果输入的矩阵不是方阵或不可逆，则抛出此异常。</exception>
        public static Matrix Inverse(this Matrix matrix, Matrix augmentedMatrix = default, Matrix inverseMatrix = default)
        {
            if (!IsSquareAndInvertible(matrix))
            {
                throw new ArgumentException("The input matrix is not a square matrix or is not invertible.");
            }

            int n = matrix.Row;
            if (augmentedMatrix.IsEmpty)
            {
                augmentedMatrix = new Matrix(n, 2 * n);
            }
            else
            {
                if (augmentedMatrix.Row != n || augmentedMatrix.Column != 2 * n)
                    throw new ArgumentException("The number of rows in the expanded matrix does not match the original matrix, or the number of columns is not twice that of the original matrix.");
            }


            // 构建增广矩阵，原矩阵在左边，单位矩阵在右边
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmentedMatrix[i, j] = matrix[i, j];
                }
                for (int j = n; j < 2 * n; j++)
                {
                    augmentedMatrix[i, j] = (j - n == i) ? 1 : 0;
                }
            }

            // 高斯-约当消元过程
            for (int i = 0; i < n; i++)
            {
                // 确保主对角线上的元素不为零，如果为零则与下面的行交换
                if (augmentedMatrix[i, i] == 0)
                {
                    bool foundNonZero = false;
                    for (int k = i + 1; k < n; k++)
                    {
                        if (augmentedMatrix[k, i] != 0)
                        {
                            SwapRows(augmentedMatrix, i, k, 2 * n);
                            foundNonZero = true;
                            break;
                        }
                    }
                    if (!foundNonZero)
                    {
                        throw new Exception("矩阵不可逆，无法找到主对角线上非零元素。");
                    }
                }

                // 将主对角线上的元素化为1
                MultiplyRow(augmentedMatrix, i, 1 / augmentedMatrix[i, i], 2 * n);

                // 将主对角线上元素所在列的其他元素化为零
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        AddRows(augmentedMatrix, j, i, -augmentedMatrix[j, i], 2 * n);
                    }
                }
            }

            // 提取逆矩阵，即增广矩阵的右边部分
            inverseMatrix = matrix.CopyTo(inverseMatrix);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    inverseMatrix[i, j] = augmentedMatrix[i, j + n];
                }
            }

            return inverseMatrix;
        }


        /// <summary>
        /// 计算两个矩阵的克罗内克积（Kronecker Product）
        /// </summary>
        /// <param name="matrix">参与运算的第一个矩阵</param>
        /// <param name="other">参与运算的第二个矩阵</param>
        /// <returns>返回两个矩阵的克罗内克积</returns>
        public static Matrix KroneckerProduct(this Matrix matrix, Matrix other)
        {
            Matrix result = new Matrix(matrix.Row * other.Row, matrix.Column * other.Column);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    for (int k = 0; k < other.Row; k++)
                    {
                        for (int l = 0; l < other.Column; l++)
                            result[(i * other.Row) + k, (j * other.Column) + l] = matrix[i, j] * other[k, l];
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 计算矩阵与向量的外积（Outer Product）
        /// </summary>
        /// <param name="matrix">参与运算的矩阵</param>
        /// <param name="other">参与运算的向量</param>
        /// <returns>返回矩阵与向量的外积</returns>
        public static Matrix OuterProduct(this Matrix matrix, double[] other)
        {
            Matrix result = new Matrix(matrix.Row, other.Length);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < other.Length; j++)
                {
                    result[i, j] = matrix[i, 0] * other[j];
                }
            }

            return result;
        }

        #endregion

        #region 矩阵操作

        /// <summary>
        /// 在矩阵最后添加一列
        /// </summary>
        /// <param name="matrix">要添加列的矩阵</param>
        /// <param name="nums">要添加的数字数组</param>
        /// <returns>添加列后的新矩阵</returns>
        public static Matrix AppendColumn(this Matrix matrix, double[] nums)
        {
            if (matrix.Row != nums.Length)
                throw new ArgumentException("The length of the new data column needs to be equal to the number of rows in the matrix.");

            var resultMatrix = new Matrix(matrix.Row, matrix.Column + 1);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    resultMatrix[i, j] = matrix[i, j];
                }
                resultMatrix[i, resultMatrix.Column - 1] = nums[i];
            }

            return resultMatrix;
        }

        /// <summary>
        /// 在矩阵最后添加一列
        /// </summary>
        /// <param name="matrix">要添加列的矩阵</param>
        /// <param name="num">要添加的数字</param>
        /// <returns>添加列后的新矩阵</returns>
        public static Matrix AppendColumn(this Matrix matrix, double num)
        {
            var resultMatrix = new Matrix(matrix.Row, matrix.Column + 1);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    resultMatrix[i, j] = matrix[i, j];
                }
                resultMatrix[i, resultMatrix.Column - 1] = num;
            }

            return resultMatrix;
        }

        /// <summary>
        /// 将一个矩阵作为列附加到另一个矩阵的右侧。
        /// </summary>
        /// <param name="matrix">要进行列附加操作的矩阵。</param>
        /// <param name="matrixY">要附加到matrix右侧的矩阵。</param>
        /// <returns>返回一个新矩阵，其中包含了matrix和matrixY的内容，matrixY的内容作为新矩阵的附加列。</returns>
        /// <exception cref="ArgumentNullException">如果matrix和matrixY的行数不相等，则抛出此异常。</exception>
        public static Matrix AppendColumn(this Matrix matrix, Matrix matrixY)
        {
            if (matrix.Row != matrixY.Row)
                throw new ArgumentException(nameof(matrixY));

            var resultMatrix = new Matrix(matrix.Row, matrix.Column + matrixY.Column);

            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    resultMatrix[i, j] = matrix[i, j];
                }
                for (int j = 0; j < matrixY.Column; j++)
                {
                    resultMatrix[i, matrix.Column + j] = matrixY[i, j];
                }
            }

            return resultMatrix;
        }

        /// <summary>
        /// 向矩阵末尾添加一行。
        /// </summary>
        /// <param name="matrix">待添加行的矩阵。</param>
        /// <param name="nums">要添加的行数据。</param>
        /// <returns>添加行后的新矩阵。</returns>
        /// <exception cref="ArgumentException">如果新数据列的长度不等于矩阵的行数，则抛出此异常。</exception>
        public static Matrix AppendRow(this Matrix matrix, double[] nums)
        {
            if (matrix.Column != nums.Length)
                throw new ArgumentException("The length of the new data column needs to be equal to the number of rows in the matrix.");

            int row = matrix.Row + 1;
            var resultMatrix = matrix.CopyTo(new Matrix(row, matrix.Column));

            for (int i = 0; i < matrix.Column; i++)
            {
                resultMatrix[matrix.Row, i] = nums[i];
            }

            return resultMatrix;
        }

        /// <summary>
        /// 在矩阵的最后一行追加一个新的行。
        /// </summary>
        /// <param name="matrix">待追加行的矩阵。</param>
        /// <param name="num">要追加的新行中的元素值。</param>
        /// <returns>返回追加新行后的矩阵。</returns>
        public static Matrix AppendRow(this Matrix matrix, double num)
        {
            int row = matrix.Row + 1;
            var resultMatrix = matrix.CopyTo(new Matrix(matrix.Row + 1, matrix.Column));

            for (int i = 0; i < matrix.Column; i++)
            {
                resultMatrix[matrix.Row, i] = num;
            }

            return resultMatrix;
        }

        /// <summary>
        /// 在矩阵的最后一行追加一个新的行。
        /// </summary>
        /// <param name="matrix">待追加行的矩阵。</param>
        /// <param name="num">要追加的新行中的元素值。</param>
        /// <returns>返回追加新行后的矩阵。</returns>
        public static Matrix AppendRow(this Matrix matrix, double num, int count)
        {
            if (count <= 0)
                throw new AggregateException(nameof(count));

            int row = matrix.Row + 1;
            var resultMatrix = matrix.CopyTo(new Matrix(matrix.Row + count, matrix.Column));

            for (int i = matrix.Row; i < resultMatrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    resultMatrix[i, j] = num;
                }
            }

            return resultMatrix;
        }

        #endregion

        #region 随机

        /// <summary>
        /// 随机生成指定行数和列数的矩阵，矩阵元素服从均匀分布且可定义范围。
        /// </summary>
        /// <param name="rows">矩阵的行数。</param>
        /// <param name="columns">矩阵的列数。</param>
        /// <param name="minValue">随机数的最小值。</param>
        /// <param name="maxValue">随机数的最大值。</param>
        /// <returns>生成的随机矩阵。</returns>
        public static Matrix NextMatrix(this Random random, int rows, int columns, double minValue = 0, double maxValue = 0)
        {
            if (random is null) random = new Random();
            var matrix = new Matrix(rows, columns);

            //差值
            double differenceValue = maxValue - minValue;
            if (differenceValue == 0) differenceValue = 1;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = random.NextDouble() * differenceValue + minValue;
                }
            }

            return matrix;
        }

        /// <summary>
        /// 在同一个矩阵内基于矩阵长度相关计算打乱其元素顺序。
        /// </summary>
        /// <param name="matrix">要打乱的矩阵。</param>
        public static Matrix ShuffleMatrixInPlaceWithoutRandom(this Matrix matrix)
        {
            int rows = matrix.Row;
            int cols = matrix.Column;
            int matrixSize = rows * cols;

            // 遍历矩阵元素，通过特定的索引变换来打乱顺序
            for (int i = 0; i < matrixSize; i++)
            {
                // 计算新的索引位置，这里采用一种简单的基于矩阵长度的变换方式
                // 例如：将索引i乘以一个基于矩阵大小的数，然后取模得到新索引
                int newIndex = (i * (matrixSize - 1)) % matrixSize;

                // 将原索引i和新索引newIndex对应的元素在矩阵中进行交换
                int rowI = i / cols;
                int colI = i % cols;
                int rowNewIndex = newIndex / cols;
                int colNewIndex = newIndex % cols;

                double temp = matrix[rowI, colI];
                matrix[rowI, colI] = matrix[rowNewIndex, colNewIndex];
                matrix[rowNewIndex, colNewIndex] = temp;
            }

            return matrix;
        }

        /// <summary>
        /// 生成线性回归测试数据
        /// </summary>
        /// <param name="random">随机数生成器</param>
        /// <param name="numSamples">样本数量</param>
        /// <param name="numIndependentVariables">独立变量的数量，默认为1</param>
        /// <param name="testRatio">测试集的比例，默认为20%</param>
        /// <param name="trueIntercept">真实截距，默认为0</param>
        /// <param name="noiseStdDev">噪声的标准差，默认为0</param>
        /// <returns>返回一个包含训练集特征矩阵、训练集标签矩阵、测试集特征矩阵、测试集标签矩阵和真实斜率数组的值元组</returns>
        /// <remarks>噪声的标准差最好在0~10之间，本测试数据生成范围在0~10之间</remarks>
        public static ValueTuple<Matrix, Matrix, Matrix, Matrix, double[]> CreateLinearRegressionTestData(this Random random, int numSamples, int numIndependentVariables = 1, int testRatio = 20, double trueIntercept = 0, double noiseStdDev = 0)
        {
            double[] trueSlopes = new double[numIndependentVariables];
            for (int i = 0; i < numIndependentVariables; i++)
            {
                trueSlopes[i] = random.NextDouble() * 10;
            }
            var result = random.CreateLinearRegressionTestData(numSamples, testRatio, trueIntercept, noiseStdDev, trueSlopes);

            return ValueTuple.Create(result.Item1, result.Item2, result.Item1, result.Item2, trueSlopes);
        }

        /// <summary>
        /// 生成线性回归测试数据
        /// </summary>
        /// <param name="random">随机数生成器</param>
        /// <param name="numSamples">样本数量</param>
        /// <param name="testRatio">测试集的比例，默认为20%</param>
        /// <param name="trueIntercept">真实截距，默认为0</param>
        /// <param name="noiseStdDev">噪声的标准差，默认为0</param>
        /// <param name="trueSlopes">真实斜率数组，默认为null</param>
        /// <returns>返回一个包含训练集特征矩阵、训练集标签矩阵、测试集特征矩阵和测试集标签矩阵的值元组</returns>
        /// <remarks>噪声的标准差最好在0~10之间，本测试数据生成范围在0~10之间</remarks>
        public static ValueTuple<Matrix, Matrix, Matrix, Matrix> CreateLinearRegressionTestData(this Random random, int numSamples, int testRatio = 20, double trueIntercept = 0, double noiseStdDev = 0, double[] trueSlopes = null)
        {
            var train = random.CreateLinearRegressionTestData(numSamples, trueSlopes, trueIntercept, noiseStdDev);
            var test = random.CreateLinearRegressionTestData(numSamples * testRatio / 100, trueSlopes, trueIntercept, noiseStdDev);

            return ValueTuple.Create(train.Item1, train.Item2, test.Item1, test.Item2);
        }

        /// <summary>
        /// 创建线性回归测试数据
        /// </summary>
        /// <param name="random">随机数生成器</param>
        /// <param name="numSamples">样本数量</param>
        /// <param name="numIndependentVariables">自变量数量</param>
        /// <param name="trueSlopes">真实的斜率数组</param>
        /// <param name="trueIntercept">真实截距，默认为0</param>
        /// <param name="noiseStdDev">噪声标准差，默认为0</param>
        /// <returns>包含自变量矩阵和因变量矩阵的元组</returns>
        public static ValueTuple<Matrix, Matrix> CreateLinearRegressionTestData(this Random random, int numSamples, double[] trueSlopes, double trueIntercept = 0, double noiseStdDev = 0)
        {
            if (numSamples == 0)
                return default;

            //真实斜率数组
            if (trueSlopes is null)
                throw new ArgumentNullException(nameof(trueSlopes));

            // 生成随机的自变量矩阵
            //给定自变量数组的自变量数据
            int numIndependentVariables = trueSlopes.Length;
            //是否有截距
            bool hasTrueIntercept = trueIntercept != 0;
            //实际自变量数量，是否需要加上截距
            int numVariables = hasTrueIntercept ? numIndependentVariables + 1 : numIndependentVariables;
            var independentVariablesMatrix = new Matrix(numSamples, numVariables);
            if (random is null) random = new Random();
            for (int i = 0; i < numSamples; i++)
            {
                for (int j = 0; j < numVariables; j++)
                {
                    independentVariablesMatrix[i, j] = random.NextDouble();
                }
            }

            //如果有斜率，则将最后一列改为1
            if (hasTrueIntercept)
            {
                for (int i = 0; i < numSamples; i++)
                {
                    independentVariablesMatrix[i, numVariables - 1] = 1;
                }
            }

            // 生成因变量数组，根据真实斜率、截距和添加噪声
            var dependentVariablesArray = new Matrix(numSamples, 1);
            for (int i = 0; i < numSamples; i++)
            {
                double predictedValue = 0.0;
                for (int j = 0; j < numIndependentVariables; j++)
                {
                    predictedValue += trueSlopes[j] * independentVariablesMatrix[i, j];
                }
                dependentVariablesArray[i, 0] = predictedValue + trueIntercept + random.NextGaussian(0, noiseStdDev);
            }

            return ValueTuple.Create(independentVariablesMatrix, dependentVariablesArray);
        }

        #endregion

        #region 误差

        /// <summary>
        /// 计算均方误差（MSE）。
        /// </summary>
        /// <param name="PredictionMatrix">预测结果矩阵。</param>
        /// <param name="TrueYMatrix">真实的因变量矩阵。</param>
        /// <returns>均方误差值。</returns>
        public static double CalculateMSE(this Matrix PredictionMatrix, Matrix TrueYMatrix)
        {
            if (PredictionMatrix.Row != TrueYMatrix.Row)
                throw new ArgumentException("预测结果矩阵和真实因变量矩阵的行数必须相同。");

            int n = PredictionMatrix.Row;
            double sumSquaredError = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < PredictionMatrix.Column; j++)
                {
                    double error = TrueYMatrix[i, j] - PredictionMatrix[i, j];
                    sumSquaredError += error * error;
                }
            }

            return sumSquaredError / n;
        }

        /// <summary>
        /// 计算平均绝对误差（MAE）。
        /// </summary>
        /// <param name="PredictionMatrix">预测结果矩阵。</param>
        /// <param name="TrueYMatrix">真实的因变量矩阵。</param>
        /// <returns>平均绝对误差值。</returns>
        public static double CalculateMAE(this Matrix PredictionMatrix, Matrix TrueYMatrix)
        {
            if (PredictionMatrix.Row != TrueYMatrix.Row)
                throw new ArgumentException("预测结果矩阵和真实因变量矩阵的行数必须相同。");

            int n = PredictionMatrix.Row;
            double sumAbsoluteError = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < PredictionMatrix.Column; j++)
                {
                    double error = TrueYMatrix[i, j] - PredictionMatrix[i, j];
                    sumAbsoluteError += System.Math.Abs(error);
                }
            }

            return sumAbsoluteError / n;
        }

        /// <summary>
        /// 计算均方根误差（RMSE）。
        /// </summary>
        /// <param name="PredictionMatrix">预测结果矩阵。</param>
        /// <param name="TrueYMatrix">真实的因变量矩阵。</param>
        /// <returns>均方根误差值。</returns>
        public static double CalculateRMSE(this Matrix PredictionMatrix, Matrix TrueYMatrix)
        {
            double mse = CalculateMSE(PredictionMatrix, TrueYMatrix);
            return System.Math.Sqrt(mse);
        }

        #endregion

        #region 归一化

        /// <summary>
        /// 对矩阵每一列进行最小-最大归一化处理
        /// </summary>
        /// <param name="matrix">输入的矩阵</param>
        /// <param name="result">结果矩阵，如果为空，则创建一个新的矩阵存储结果</param>
        /// <returns>归一化后的矩阵</returns>
        /// <exception cref="ArgumentException">当输入的矩阵为空时抛出</exception>
        public static Matrix MinMaxNormalizationColumn(this Matrix matrix, Matrix result = default)
        {
            if (matrix.IsEmpty)
            {
                throw new ArgumentException("输入矩阵不能为空", "data");
            }

            if (result.IsEmpty || result.Row != matrix.Row || result.Column != matrix.Column)
                result = new Matrix(matrix.Row, matrix.Column);

            for (int i = 0; i < matrix.Column; i++)
            {
                double min = 0.0;
                double max = 0.0;
                // 找到数据中的最小值和最大值
                for (int j = 0; j < matrix.Row; j++)
                {
                    double num = matrix[j, i];
                    if (num < min)
                    {
                        min = num;
                    }
                    else if (num > max)
                    {
                        max = num;
                    }
                }

                //计算归一化
                double diff = max - min;
                for (int j = 0; j < matrix.Row; j++)
                {
                    result[j, i] = (matrix[j, i] - min) / diff;
                }
            }

            return result;
        }

        /// <summary>
        /// 对矩阵每一行进行最小-最大归一化处理
        /// </summary>
        /// <param name="matrix">输入的矩阵</param>
        /// <param name="result">结果矩阵，如果为空，则创建一个新的矩阵存储结果</param>
        /// <returns>归一化后的矩阵</returns>
        /// <exception cref="ArgumentException">当输入的矩阵为空时抛出</exception>
        public static Matrix MinMaxNormalizationRow(this Matrix matrix, Matrix result = default)
        {
            if (matrix.IsEmpty)
            {
                throw new ArgumentException("输入矩阵不能为空", "data");
            }

            if (result.IsEmpty || result.Row != matrix.Row || result.Column != matrix.Column)
                result = new Matrix(matrix.Row, matrix.Column);

            for (int i = 0; i < matrix.Row; i++)
            {
                double min = 0.0;
                double max = 0.0;
                // 找到数据中的最小值和最大值
                for (int j = 0; j < matrix.Column; j++)
                {
                    double num = matrix[i, j];
                    if (num < min)
                    {
                        min = num;
                    }
                    else if (num > max)
                    {
                        max = num;
                    }
                }

                //计算归一化
                double diff = max - min;
                for (int j = 0; j < matrix.Column; j++)
                {
                    result[i, j] = (matrix[i, j] - min) / diff;
                }
            }

            return result;
        }

        /// <summary>
        /// 对矩阵的每一列进行Z分数归一化处理
        /// </summary>
        /// <param name="matrix">待处理的矩阵</param>
        /// <param name="result">结果矩阵，如果为空，则创建一个新的矩阵来存储结果</param>
        /// <returns>归一化后的矩阵</returns>
        /// <exception cref="ArgumentException">当输入矩阵为空时抛出异常</exception>
        public static Matrix ZScoreNormalizationColumn(this Matrix matrix, Matrix result = default)
        {
            if (matrix.IsEmpty)
            {
                throw new ArgumentException("输入矩阵不能为空", "data");
            }

            if (result.IsEmpty || result.Row != matrix.Row || result.Column != matrix.Column)
                result = new Matrix(matrix.Row, matrix.Column);

            int row = matrix.Row;
            for (int i = 0; i < matrix.Column; i++)
            {
                double sum = 0;
                // 计算数据的总和
                for (int j = 0; j < row; j++)
                {
                    sum += matrix[j, i];
                }

                double mean = sum / row;

                double sumSquaredDiff = 0;
                // 计算数据与均值的差的平方和
                for (int j = 0; j < row; j++)
                {
                    double diff = matrix[j, i] - mean;
                    sumSquaredDiff += diff * diff;
                }

                double stdDeviation = System.Math.Sqrt(sumSquaredDiff / row);

                //计算归一化
                //如果这一列全为相同数时候会出现零除零，直接让他相等就行
                if (stdDeviation == 0)
                {
                    for (int j = 0; j < row; j++)
                    {
                        result[j, i] = 1;
                    }
                }
                else
                {
                    for (int j = 0; j < row; j++)
                    {
                        result[j, i] = (matrix[j, i] - mean) / stdDeviation;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 对矩阵的行进行Z分数归一化处理
        /// </summary>
        /// <param name="matrix">待处理的矩阵</param>
        /// <param name="result">存储结果的矩阵，默认为null</param>
        /// <returns>归一化后的矩阵</returns>
        /// <exception cref="ArgumentException">当输入矩阵为空时抛出</exception>
        public static Matrix ZScoreNormalizationRow(this Matrix matrix, Matrix result = default)
        {
            if (matrix.IsEmpty)
            {
                throw new ArgumentException("输入矩阵不能为空", "data");
            }

            if (result.IsEmpty || result.Row != matrix.Row || result.Column != matrix.Column)
                result = new Matrix(matrix.Row, matrix.Column);

            int column = matrix.Column;
            for (int i = 0; i < matrix.Row; i++)
            {
                double sum = 0;
                // 计算数据的总和
                for (int j = 0; j < column; j++)
                {
                    sum += matrix[i, j];
                }

                double mean = sum / column;

                double sumSquaredDiff = 0;
                // 计算数据与均值的差的平方和
                for (int j = 0; j < column; j++)
                {
                    double diff = matrix[i, j] - mean;
                    sumSquaredDiff += diff * diff;
                }

                double stdDeviation = System.Math.Sqrt(sumSquaredDiff / column);

                //计算归一化
                for (int j = 0; j < column; j++)
                {
                    result[i, j] = (matrix[i, j] - mean) / stdDeviation;
                }

            }

            return result;
        }

        #endregion
    }
}
