
using ExtenderApp.Data;
using ExtenderApp.Common.Math;

namespace MachineLearning
{
    /// <summary>
    /// 线性回归类，用于根据一组数据点计算线性回归模型的参数，并允许进行预测和模型性能评估。
    /// </summary>
    public class LinearRegression
    {
        /// <summary>
        /// 获取一个值，该值指示是否需要计算截距。
        /// </summary>
        public bool InterceptRequired { get; }

        /// <summary>
        /// 获取计算得到的截距值。
        /// </summary>
        public double Intercept { get; private set; }

        /// <summary>
        /// 训练数据集
        /// </summary>
        public Matrix MatrixX { get; private set; }
        /// <summary>
        /// 结果集
        /// </summary>
        public Matrix MatrixY { get; private set; }

        /// <summary>
        /// 获取计算得到的系数矩阵。
        /// </summary>
        public Matrix CoefficientMatrix { get; private set; }

        /// <summary>
        /// 初始化 LinearRegression 类的新实例。
        /// </summary>
        /// <param name="interceptRequired">指示是否需要计算截距。</param>
        public LinearRegression(bool interceptRequired = false)
        {
            InterceptRequired = interceptRequired;
        }

        /// <summary>
        /// 对给定的矩阵和值数组进行线性回归分析。
        /// </summary>
        /// <param name="matrix">输入的自变量矩阵。</param>
        /// <param name="doubles">因变量的值数组。</param>
        public void Exercise(Matrix matrix, double[] doubles)
        {
            Exercise(matrix, new Matrix(doubles));
        }

        /// <summary>
        /// 对给定的矩阵和二维数组进行线性回归分析。
        /// </summary>
        /// <param name="matrix">输入的自变量矩阵。</param>
        /// <param name="matrixY">因变量的二维数组。</param>
        public void Exercise(Matrix matrix, double[,] matrixY)
        {
            Exercise(matrix, new Matrix(matrixY));
        }

        /// <summary>
        /// 对给定的矩阵和矩阵进行线性回归分析。
        /// </summary>
        /// <param name="matrixX">输入的自变量矩阵。</param>
        /// <param name="matrixY">因变量的矩阵。</param>
        public void Exercise(Matrix matrixX, Matrix matrixY)
        {
            MatrixX = matrixX;
            MatrixY = matrixY;
            if (InterceptRequired)
            {
                CalculateInterceptCoefficient(matrixX, matrixY);
            }
            else
            {
                CalculateCoefficient(matrixX, matrixY);
            }
        }

        /// <summary>
        /// 计算线性回归的系数矩阵（不包括截距）。
        /// </summary>
        /// <param name="matrix">输入的自变量矩阵。</param>
        /// <param name="matrixY">因变量的矩阵。</param>
        private void CalculateCoefficient(Matrix matrix, Matrix matrixY)
        {
            var transpose = matrix.Transpose();

            CoefficientMatrix = transpose.Dot(matrix).Inverse().Dot(transpose).Dot(matrixY);
        }

        /// <summary>
        /// 计算线性回归的系数矩阵（包括截距）。
        /// </summary>
        /// <param name="matrix">输入的自变量矩阵。</param>
        /// <param name="matrixY">因变量的矩阵。</param>
        private void CalculateInterceptCoefficient(Matrix matrix, Matrix matrixY)
        {
            //Matrix interceptMatrix = matrix;
            // 在X矩阵的最右边添加一列全为1的列，用于计算截距
            //if (!IsLastColumnAllOnes(matrix))
            //{
            //    interceptMatrix = matrix.AppendColumn(1);
            //    if (matrix.Row + 1 != matrixY.Column) matrixY = matrixY.AppendColumn(0);
            //}
            //else
            //{
            //    if (matrix.Row != matrixY.Column) matrixY = matrixY.AppendColumn(0);
            //    interceptMatrix = matrix;
            //}

            // 如果添加截距列后矩阵的行数不等于因变量矩阵的行数，需要对因变量矩阵进行处理
            if (matrix.Row != matrixY.Row)
            {
                matrixY = matrixY.AppendRow(0, matrix.Row - matrixY.Row);
            }

            //默认最后为截距值
            // 计算 (X^T * X) 的逆矩阵
            var transpose = matrix.Transpose();
            var inverseMatrix = transpose.Dot(matrix).Inverse();

            // 计算系数向量，其中包含截距和斜率
            CoefficientMatrix = inverseMatrix.Dot(transpose).Dot(matrixY);

            //赋值截距
            Intercept = CoefficientMatrix[CoefficientMatrix.Row - 1, 0];
        }

        /// <summary>
        /// 检查矩阵的最后一列是否全部为1。
        /// </summary>
        /// <param name="matrix">要检查的矩阵。</param>
        /// <returns>如果最后一列全部为1，则返回 true；否则返回 false。</returns>
        private bool IsLastColumnAllOnes(Matrix matrix)
        {
            for (int i = 0; i < matrix.Row; i++)
            {
                if (matrix[i, matrix.Column - 1] != 1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 使用系数矩阵对单个数值进行预测。
        /// </summary>
        /// <param name="num">待预测的数值。</param>
        /// <returns>预测结果矩阵，维度与因变量维度相同。</returns>
        /// <exception cref="ArgumentException">如果系数矩阵的行数与 1 不相等或系数矩阵的列数与因变量维度不相等，则抛出此异常。</returns>
        public double Prediction(double num)
        {
            if (CoefficientMatrix.Row != 1 || CoefficientMatrix.Column != MatrixY.Column)
                throw new ArgumentException(nameof(Prediction));

            Matrix inputMatrix = new Matrix(num);
            var transpose = inputMatrix.Transpose();

            return transpose.Dot(CoefficientMatrix)[0, 0];
        }

        /// <summary>
        /// 使用系数矩阵对一组数值进行预测。
        /// </summary>
        /// <param name="nums">待预测的数值数组。</returns>
        /// <returns>预测结果矩阵，维度与因变量维度相同。</returns>
        /// <exception cref="ArgumentException">如果系数矩阵的行数与待预测数值数组的长度不相等或系数矩阵的列数与因变量维度不相等，则抛出此异常。</returns>
        public Matrix Prediction(double[] nums)
        {
            if (CoefficientMatrix.Row != nums.Length || CoefficientMatrix.Column != MatrixY.Column)
                throw new ArgumentException(nameof(Prediction));

            Matrix inputMatrix = new Matrix(nums);
            var transpose = inputMatrix.Transpose();

            return transpose.Dot(CoefficientMatrix);
        }

        /// <summary>
        /// 使用系数矩阵对矩阵进行预测。
        /// </summary>
        /// <param name="matrix">待预测的矩阵。</returns>
        /// <returns>预测结果矩阵，维度与因变量维度相同。</returns>
        public Matrix Prediction(Matrix matrix)
        {
            if (CoefficientMatrix.Row != matrix.Row || CoefficientMatrix.Column != MatrixY.Column)
                throw new ArgumentException(nameof(Prediction));

            var transpose = matrix.Transpose();

            return transpose.Dot(CoefficientMatrix);
        }

        /// <summary>
        /// 计算每个因变量维度的均方误差（MSE）。
        /// </summary>
        /// <returns>返回一个数组，数组中的每个元素是对应因变量维度的均方误差。</returns>
        public double[] CalculateMSE()
        {
            double[] mseArray = new double[MatrixY.Column];

            for (int j = 0; j < MatrixY.Column; j++)
            {
                double sumSquaredError = 0;
                for (int i = 0; i < MatrixY.Row; i++)
                {
                    Matrix prediction = Prediction(MatrixX);
                    double error = MatrixY[i, j] - prediction[i, j];
                    sumSquaredError += error * error;
                }

                mseArray[j] = sumSquaredError / MatrixY.Row;
            }

            return mseArray;
        }

        /// <summary>
        /// 计算平均绝对误差（MAE）。
        /// </summary>
        /// <returns>返回平均绝对误差的值。</returns>
        public double CalculateMAE()
        {
            double sumAbsoluteError = 0;

            for (int j = 0; j < MatrixY.Column; j++)
            {
                for (int i = 0; i < MatrixY.Row; i++)
                {
                    Matrix prediction = Prediction(MatrixX);
                    double error = MatrixY[i, j] - prediction[i, j];
                    sumAbsoluteError += Math.Abs(error);
                }
            }

            return sumAbsoluteError / (MatrixY.Row * MatrixY.Column);
        }

        /// <summary>
        /// 计算决定系数（R²）。
        /// </summary>
        /// <returns>返回决定系数的值。</returns>
        public double CalculateR2()
        {
            double[] mseArray = CalculateMSE();
            double totalSumSquaredError = 0;
            double totalSumSquaredResidual = 0;

            for (int j = 0; j < MatrixY.Column; j++)
            {
                for (int i = 0; i < MatrixY.Row; i++)
                {
                    Matrix prediction = Prediction(MatrixX);
                    double error = MatrixY[i, j] - prediction[i, j];
                    totalSumSquaredError += error * error;
                    double residual = MatrixY[i, j] - MatrixY[i, j];
                    totalSumSquaredResidual += residual * residual;
                }
            }

            return 1 - (totalSumSquaredError / totalSumSquaredResidual);
        }
    }
}
