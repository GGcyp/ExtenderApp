using ExtenderApp.Data;
using ExtenderApp.Common.Math;

namespace MachineLearning
{
    public class RidgeRegression : BaseMachineLearning
    {
        /// <summary>
        /// 正则化参数lambda
        /// </summary>
        public double Lambda { get; set; }

        /// <summary>
        /// 构造函数，初始化岭回归的正则化参数
        /// </summary>
        /// <param name="lambda">正则化参数值</param>
        public RidgeRegression(double lambda) : base(true)
        {
            this.Lambda = lambda;
        }

        public override void DataFit(Matrix matrixX, Matrix matrixY)
        {
            base.DataFit(matrixX, matrixY);

            Matrix transpose = matrixX.Transpose();
            var identityMatrix = Matrix.Identity(matrixX.Column, 1);
            // 计算 (X^T * X + lambda * I)^(-1) 原生加减乘法会产生新的内存消耗
            var inverseMatrix = (transpose + identityMatrix.Multiplication(Lambda)).Inverse();
            // 计算 (X^T * X + lambda * I)^(-1) * X^T
            var result = inverseMatrix.Multiplication(transpose);

            CoefficientMatrix = result.Dot(matrixY);
        }
    }
}
