using ExtenderApp.Data;

namespace MachineLearning.Linear
{
    /// <summary>
    /// 逻辑回归
    /// </summary>
    public class LogisticRegression : LinearRegression
    {
        public override Matrix Prediction(Matrix matrix)
        {
            var result = base.Prediction(matrix);

            for (int i = 0; i < result.Row; i++)
            {
                result[i, 0] = 1 / (1 + System.Math.Pow(Math.E, -result[i, 0]));
            }

            return result;
        }

        /// <summary>
        /// 检查损失函数值
        /// </summary>
        /// <param name="matrix">输入矩阵</param>
        /// <param name="minValue">最小损失值，默认为0.000001</param>
        /// <param name="maxValue">最大损失值，默认为0.999999</param>
        /// <returns>返回损失值</returns>
        public double InspectLoss(Matrix matrix, double minValue = 0.00000001, double maxValue = 0.99999999)
        {
            var result = Prediction(matrix);
            double loss = 0.0;
            for (int i = 0; i < result.Row; i++)
            {
                var y = result[i, 0];
                var p = System.Math.Max(y, minValue);
                p = System.Math.Min(p, maxValue);
                loss += -(y * System.Math.Log(p) + (1 - y) * System.Math.Log(1 - p));
            }

            return loss;
        }
    }
}
