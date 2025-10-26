using ExtenderApp.Data;
using ExtenderApp.Common.Math;

namespace ExtenderApp.Common.MachineLearning
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
        public RidgeRegression(double lambda = 0.1)
        {
            if (lambda < 0) 
                throw new ArgumentNullException(nameof(lambda));
            this.Lambda = lambda;
        }

        public override void DataFit()
        {
            Matrix transpose = MatrixX.Transpose();
            var identityMatrix = Matrix.Identity(MatrixX.Column);
            // 计算 (X^TLinkClient * X + lambda * I)^(-1) 原生加减乘法会产生新的内存消耗
            var inverseMatrix = (transpose * MatrixX + identityMatrix.Multiplication(Lambda)).Inverse();
            // 计算 (X^TLinkClient * X + lambda * I)^(-1) * X^TLinkClient
            var result = inverseMatrix.Multiplication(transpose);

            CoefficientMatrix = result.Dot(MatrixY);
        }
    }
}
