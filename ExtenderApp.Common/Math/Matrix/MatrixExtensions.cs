using ExtenderApp.Data;

namespace ExtenderApp.Common.Math
{
    public static class MatrixExtensions
    {
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
        /// 对两个矩阵进行点乘运算。
        /// </summary>
        /// <param name="matrixLeft">左侧矩阵。</param>
        /// <param name="matrixRight">右侧矩阵。</param>
        /// <param name="canKroneckerProduct">是否允许克罗内克积运算，默认为false。</param>
        /// <returns>返回点乘后的矩阵。</returns>
        /// <exception cref="ArgumentNullException">如果输入的矩阵为空，则抛出此异常。</exception>
        /// <exception cref="ArgumentException">如果两个矩阵不是同型矩阵且不允许克罗内克积运算，则抛出此异常。</exception>
        public static Matrix Dot(this Matrix matrixLeft, Matrix matrixRight, bool canKroneckerProduct = true)
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
            Matrix resultMatrix = matrixLeft * matrixRight;
            //Matrix resultMatrix = new Matrix(matrixLeft.Row, matrixLeft.Column);

            //// 执行矩阵点乘运算
            //for (int i = 0; i < matrixLeft.Row; i++)
            //{
            //    for (int j = 0; j < matrixLeft.Column; j++)
            //    {
            //        resultMatrix[i, j] = matrixLeft[i, j] * matrixRight[i, j];
            //    }
            //}

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

    }
}
