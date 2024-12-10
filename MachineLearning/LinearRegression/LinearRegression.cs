using ExtenderApp.Data;
using ExtenderApp.Common.Math;
using System.Text;

namespace MachineLearning.Linear
{
    /// <summary>
    /// 线性回归类，用于根据一组数据点计算线性回归模型的参数，并允许进行预测和模型性能评估。
    /// </summary>
    public class LinearRegression : BaseMachineLearning
    {
        public LinearRegression(bool interceptRequired = true)
        {
        }

        public override void DataFit()
        {
            //默认最后为截距值
            // 计算 (X^T * X + lambda * I)^(-1) 的逆矩阵
            var transpose = MatrixX.Transpose();
            var inverseMatrix = transpose.Dot(MatrixX).Inverse();

            // 计算系数向量，其中包含截距和斜率(X^T * X + lambda * I)^(-1) * X^T
            CoefficientMatrix = inverseMatrix.Dot(transpose).Dot(MatrixY);
        }
    }
}
