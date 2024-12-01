
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
        /// <param name="matrix">输入的自变量矩阵。</param>
        /// <param name="matrixY">因变量的矩阵。</param>
        public void Exercise(Matrix matrix, Matrix matrixY)
        {
            if (InterceptRequired)
            {
                CalculateInterceptCoefficient(matrix, matrixY);
            }
            else
            {
                CalculateCoefficient(matrix, matrixY);
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

            //默认最后为截距值
            // 计算 (X^T * X) 的逆矩阵
            var inverseMatrix = matrix.Transpose().Dot(matrix).Inverse();

            // 计算系数向量，其中包含截距和斜率
            CoefficientMatrix = inverseMatrix.Dot(matrix).Dot(matrixY);

            //赋值截距
            Intercept = CoefficientMatrix[CoefficientMatrix.Row - 1, CoefficientMatrix.Column - 1];
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

        public double Prediction(double num)
        {
            return 0.0;
        }
    }
}
