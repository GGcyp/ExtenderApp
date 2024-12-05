using ExtenderApp.Common.Math;
using ExtenderApp.Data;

namespace MachineLearning
{
    /// <summary>
    /// 全量梯度下降算法类,BGD,所有数据计算类
    /// </summary>
    public class GradientDescent_BGD : GradientDescent
    {
        public GradientDescent_BGD(double learningRate, int epochCount) : base(learningRate, epochCount)
        {
        }

        public override void DataFit(Matrix matrixX, Matrix matrixY)
        {
            base.DataFit(matrixX, matrixY);

            //梯度
            Matrix gradient;
            var transpose = MatrixX.Transpose();

            //临时存储矩阵
            Matrix xDot = new Matrix(MatrixX.Column, CoefficientMatrix.Column);
            Matrix thetaDot = new Matrix(MatrixX.Row, CoefficientMatrix.Column);

            for (int i = 0; i < EpochCount; i++)
            {
                //批量梯度下降，X矩阵包含所有的数据
                //计算梯度
                gradient = transpose.Dot(MatrixX.Dot(CoefficientMatrix, thetaDot).Sub(MatrixY), xDot);
                //更新系数
                CoefficientMatrix = CoefficientMatrix.Sub(gradient.Multiplication(LearningRate));

                UpdateLearningRateFixedDecay();
            }
        }
    }
}
