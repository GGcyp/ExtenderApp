using ExtenderApp.Data;
using ExtenderApp.Common.Math;

namespace ExtenderApp.Common.MachineLearning
{
    /// <summary>
    /// 批量梯度下降
    /// </summary>
    public class GradientDescent_MBGD : GradientDescent
    {
        /// <summary>
        /// 进行计算的矩阵
        /// </summary>
        private Matrix calculateMatrixX;

        /// <summary>
        /// 进行计算矩阵的结果
        /// </summary>
        private Matrix calculateMatrixY;

        private int randomCount;

        public GradientDescent_MBGD(double learningRate, int epochCount, Matrix theta = default, int randomCount = 1, int decayStep = 8, double decayRate = 0.99) : base(learningRate, epochCount, theta, decayStep, decayRate)
        {
            this.randomCount = randomCount;
        }

        public override void DataFit()
        {
            calculateMatrixX = new Matrix(randomCount, MatrixX.Column);
            calculateMatrixY = new Matrix(randomCount, 1);

            //梯度
            Matrix gradient;
            Matrix transpose = new Matrix(calculateMatrixX.Column, calculateMatrixX.Row);

            //临时存储矩阵
            Matrix xDot = new Matrix(calculateMatrixX.Column, CoefficientMatrix.Column);
            Matrix thetaDot = new Matrix(calculateMatrixX.Row, CoefficientMatrix.Column);

            for (int i = 0; i < EpochCount; i++)
            {
                RandomCalculateMatrix();

                //计算梯度
                gradient = calculateMatrixX.Transpose(transpose).Dot(calculateMatrixX.Dot(CoefficientMatrix, thetaDot).Sub(calculateMatrixY), xDot);
                //更新系数

                CoefficientMatrix = CoefficientMatrix.Sub(gradient.Multiplication(LearningRate));

                UpdateLearningRateFixedDecay();
                //UpdateLearningRateExponentialDecay();
            }
        }

        /// <summary>
        /// 随机计算矩阵
        /// </summary>
        /// <remarks>
        /// 该方法用于随机计算矩阵。它会在MatrixX中随机选择一行，并将这一行复制到calculateMatrixX中，
        /// 同时将MatrixY中对应行的第一列复制到calculateMatrixY中。
        /// </remarks>
        private void RandomCalculateMatrix()
        {
            for (int i = 0; i < calculateMatrixX.Row; i++)
            {
                int index = Random.Next(0, MatrixX.Row - 1);
                for (int j = 0; j < calculateMatrixX.Column; j++)
                {
                    calculateMatrixX[i, j] = MatrixX[index, j];
                }
                calculateMatrixY[i, 0] = MatrixY[index, 0];
            }
        }
    }
}
