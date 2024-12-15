using System.Data;
using System.Diagnostics;
using System.Text;
using ExtenderApp.Common.Math;
using ExtenderApp.Data;

namespace ExtenderApp.Common.MachineLearning
{
    /// <summary>
    /// 线性回归扩展函数
    /// </summary>
    public static class BaseMachineLearningExtensions
    {
        #region 常用方法

        /// <summary>
        /// 异步执行数据拟合操作
        /// </summary>
        /// <param name="regression">机器学习基础对象</param>
        /// <param name="matrixX">特征矩阵</param>
        /// <param name="matrixY">标签矩阵</param>
        /// <param name="callback">操作完成后的回调函数，参数为机器学习基础对象</param>
        /// <param name="needTiming">是否需要计时</param>
        /// <returns>异步任务</returns>
        /// <exception cref="ArgumentNullException">当 regression 参数为空时抛出</exception>
        public static async Task DataFitAsync(this BaseMachineLearning regression, Matrix matrixX, Matrix matrixY, Action<BaseMachineLearning> callback = null, bool needTiming = false)
        {
            if (regression == null)
                throw new ArgumentNullException(nameof(regression));

            Action action = needTiming ? 
                () => regression.DataFit(matrixX, matrixY, new Stopwatch()) 
                : () => regression.DataFit(matrixX, matrixY);

            await Task.Run(action);

            callback?.Invoke(regression);
        }

        /// <summary>
        /// 执行数据拟合操作
        /// </summary>
        /// <param name="regression">机器学习基础对象</param>
        /// <param name="matrixX">特征矩阵</param>
        /// <param name="matrixY">标签矩阵</param>
        /// <param name="stopwatch">计时器</param>
        /// <exception cref="ArgumentNullException">当 regression 参数为空时抛出</exception>
        public static void DataFit(this BaseMachineLearning regression, Matrix matrixX, Matrix matrixY, Stopwatch stopwatch)
        {
            if (regression == null)
                throw new ArgumentNullException(nameof(regression));

            if (stopwatch == null)
                stopwatch = new();

            stopwatch.Start();
            regression.DataFit();
            stopwatch.Stop();
        }

        #endregion

        #region 测试

        /// <summary>
        /// 为指定的机器学习模型生成测试数据，并拟合并打印结果。
        /// </summary>
        /// <param name="regression">要生成测试数据的机器学习模型。</param>
        /// <param name="random">随机数生成器，如果为null，则使用新的Random实例。</param>
        /// <param name="numSamples">要生成的样本数量，默认为500。</param>
        /// <param name="numIndependentVariables">独立变量的数量，默认为10。</param>
        /// <param name="testRatio">测试集占比，默认为20%。</param>
        /// <param name="trueIntercept">真实截距，默认为0。</param>
        /// <param name="noiseStdDev">噪声标准差，默认为0。</param>
        public static void CreateTestData(this BaseMachineLearning regression, Random random = null, int numSamples = 500, int numIndependentVariables = 10, int testRatio = 20, double trueIntercept = 0, double noiseStdDev = 0)
        {
            if (regression == null)
                throw new ArgumentNullException(nameof(regression));

            if (random is null)
                random = new Random();

            var temp = random.CreateMatrixTestData(numSamples, numIndependentVariables, testRatio, trueIntercept, noiseStdDev);
            var x = temp.Item1;
            var y = temp.Item2;

            regression.DataFit(x, y);
            regression.PrintTestData(temp);
        }

        /// <summary>
        /// 为指定的机器学习模型生成测试数据，并拟合并打印结果。
        /// </summary>
        /// <param name="regression">要生成测试数据的机器学习模型。</param>
        /// <param name="numSamples">要生成的样本数量。</param>
        /// <param name="trueSlopes">真实的斜率数组。</param>
        /// <param name="random">随机数生成器，如果为null，则使用新的Random实例。</param>
        /// <param name="testRatio">测试集占比，默认为20%。</param>
        /// <param name="trueIntercept">真实截距，默认为0。</param>
        /// <param name="noiseStdDev">噪声标准差，默认为0。</param>
        public static void CreateTestData(this BaseMachineLearning regression, int numSamples, double[] trueSlopes, Random random = null, int testRatio = 20, double trueIntercept = 0, double noiseStdDev = 0)
        {
            if (regression == null)
                throw new ArgumentNullException(nameof(regression));

            if (random is null)
                random = new Random();

            var temp = random.CreateLinearRegressionTestData(numSamples, testRatio, trueIntercept, noiseStdDev, trueSlopes);
            var x = temp.Item1;
            var y = temp.Item2;

            regression.DataFit(x, y);
            regression.PrintTestData(ValueTuple.Create(temp.Item1, temp.Item2, temp.Item3, temp.Item4, trueSlopes));
        }

        /// <summary>
        /// 打印测试数据的相关统计信息。
        /// </summary>
        /// <param name="linear">线性回归模型。</param>
        /// <param name="data">包含测试数据的元组。</param>
        private static void PrintTestData(this BaseMachineLearning linear, ValueTuple<Matrix, Matrix, Matrix, Matrix, double[]> data)
        {
            Debug.Print(linear.ToString());

            StringBuilder sb = new StringBuilder();
            var trueSlopes = data.Item5;
            sb.Append("预测系数");
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

        #endregion
    }
}
