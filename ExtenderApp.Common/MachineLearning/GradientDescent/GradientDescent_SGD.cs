using ExtenderApp.Data;
using ExtenderApp.Common.Math;

namespace ExtenderApp.Common.MachineLearning
{
    /// <summary>
    /// 随机梯度下降矩阵
    /// </summary>
    ///<remarks>经典随机梯度下降算法,每次只用一个矩阵进行计算</remarks>
    public class GradientDescent_SGD : GradientDescent
    {
        /// <summary>
        /// 进行计算的矩阵
        /// </summary>
        private Matrix calculateMatrixX;

        /// <summary>
        /// 进行计算矩阵的结果
        /// </summary>
        private Matrix calculateMatrixY;

        /// <summary>
        /// 初始化 GradientDescent_SGD 类的新实例
        /// </summary>
        /// <param name="matrixX">特征矩阵</param>
        /// <param name="matrixY">目标值矩阵</param>
        /// <param name="learningRate">学习率</param>
        /// <param name="epochCount">迭代次数</param>
        /// <param name="theta">初始参数向量，默认为默认矩阵</param>
        /// <param name="decayStep">衰减步长，衰减步长为总循环次数的n分之一,默认为10</param>
        /// <param name="decayRate">衰减率，默认为0.99</param>
        public GradientDescent_SGD(double learningRate, int epochCount, Matrix theta, int decayStep = 8, double decayRate = 0.99) : base(learningRate, epochCount, theta, decayStep, decayRate)
        {

        }

        public override void DataFit()
        {
            calculateMatrixX = new Matrix(1, MatrixX.Column);
            calculateMatrixY = new Matrix(1, 1);

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
            int index = Random.Next(0, MatrixX.Row - 1);
            for (int j = 0; j < calculateMatrixX.Column; j++)
            {
                calculateMatrixX[0, j] = MatrixX[index, j];
            }
            calculateMatrixY[0, 0] = MatrixY[index, 0];
        }
    }
}
