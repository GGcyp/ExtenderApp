using System.Diagnostics;
using System.Text;
using ExtenderApp.Common.Math;
using ExtenderApp.Data;
using MachineLearning.Linear;

namespace MachineLearning
{
    /// <summary>
    /// 线性回归扩展函数
    /// </summary>
    public static class LinearRegressionExtensions
    {
        public static void CreateTestData(this LinearRegression linearRegression, Random random = null, int numSamples = 500, int numIndependentVariables = 10, int testRatio = 20, double trueIntercept = 0, double noiseStdDev = 0)
        {
            if (linearRegression == null)
                linearRegression = new LinearRegression();

            if (random is null) random = new Random();
            var temp = random.CreateLinearRegressionTestData(numSamples, numIndependentVariables, testRatio, trueIntercept, noiseStdDev);
            var x = temp.Item1;
            var y = temp.Item2;
            LinearRegression linear = new(true);
            linear.DataFit(x, y);

            linear.PrintTestData(temp);
        }

        public static void CreateTestData(this LinearRegression linearRegression, int numSamples, double[] trueSlopes, Random random = null, int testRatio = 20, double trueIntercept = 0, double noiseStdDev = 0)
        {
            if (linearRegression == null)
                linearRegression = new LinearRegression();

            if (random is null) random = new Random();
            var temp = random.CreateLinearRegressionTestData(numSamples, testRatio, trueIntercept, noiseStdDev, trueSlopes);
            var x = temp.Item1;
            var y = temp.Item2;
            LinearRegression linear = new(true);
            linear.DataFit(x, y);


            linear.PrintTestData(ValueTuple.Create(temp.Item1, temp.Item2, temp.Item3, temp.Item4, trueSlopes));
        }

        private static void PrintTestData(this LinearRegression linear, ValueTuple<Matrix, Matrix, Matrix, Matrix, double[]> data)
        {
            Debug.Print(linear.ToString());

            StringBuilder sb = new StringBuilder();
            var trueSlopes = data.Item5;
            sb.Append("真实系数");
            sb.Append('[');
            sb.Append(trueSlopes[0].ToString());
            for (int i = 1; i < trueSlopes.Length; i++)
            {
                sb.Append('、');
                sb.Append(trueSlopes[i].ToString());
            }
            sb.Append(']');
            sb.AppendLine();
            Debug.Print(sb.ToString());

            var y_ = linear.Prediction(data.Item3);
            var mae = y_.CalculateMAE(data.Item4);
            Debug.Print(nameof(mae) + " : " + mae.ToString());
            var mse = y_.CalculateMSE(data.Item4);
            Debug.Print(nameof(mse) + " : " + mse.ToString());
            var rmse = y_.CalculateRMSE(data.Item4);
            Debug.Print(nameof(rmse) + " : " + rmse.ToString());
        }
    }
}
