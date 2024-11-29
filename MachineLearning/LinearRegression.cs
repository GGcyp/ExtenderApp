
namespace MachineLearning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 线性回归类，用于根据一组数据点计算线性回归模型的参数，并允许进行预测和模型性能评估。
    /// </summary>
    public class LinearRegression
    {
        /// <summary>
        /// 获取线性回归模型的斜率。
        /// </summary>
        public double Slope { get; private set; }

        /// <summary>
        /// 获取线性回归模型的截距。
        /// </summary>
        public double Intercept { get; private set; }

        /// <summary>
        /// 构造函数，接收数据点并计算线性回归模型的参数。
        /// </summary>
        /// <param name="dataPoints">包含数据点的列表，每个数据点由X和Y坐标组成。</param>
        public LinearRegression(List<(double X, double Y)> dataPoints)
        {
            Init(dataPoints);
        }

        /// <summary>
        /// 构造函数，根据指定的数据量、最小值和最大值生成随机数据点，并计算线性回归模型的参数。
        /// </summary>
        /// <param name="dataVolume">数据点的数量。</param>
        /// <param name="minValue">数据点的最小值。</param>
        /// <param name="maxValue">数据点的最大值。</param>
        public LinearRegression(int dataVolume, int minValue, int maxValue)
        {
            List<(double X, double Y)> dataPoints = new(dataVolume);
            Random random = new Random();
            for (int i = 0; i < dataVolume; i++)
            {
                double x = random.Next(minValue, maxValue + 1);
                double y = random.Next(minValue, maxValue + 1);
                dataPoints.Add((x, y));
            }
            Init(dataPoints);
        }

        /// <summary>
        /// 初始化方法，根据数据点计算线性回归模型的参数。
        /// </summary>
        /// <param name="dataPoints">包含数据点的列表，每个数据点由X和Y坐标组成。</param>
        private void Init(List<(double X, double Y)> dataPoints)
        {
            if (dataPoints == null || dataPoints.Count < 2)
            {
                throw new ArgumentException("数据点列表不能为空且必须包含至少两个数据点。");
            }

            // 计算均值
            double meanX = dataPoints.Average(p => p.X);
            double meanY = dataPoints.Average(p => p.Y);

            // 计算斜率m和截距b
            double numerator = dataPoints.Sum(p => (p.X - meanX) * (p.Y - meanY));
            double denominator = dataPoints.Sum(p => (p.X - meanX) * (p.X - meanX));
            if (denominator == 0)
            {
                throw new InvalidOperationException("无法计算斜率，因为分母为零（可能是所有X值都相同）。");
            }

            Slope = numerator / denominator;
            Intercept = meanY - Slope * meanX;
        }

        /// <summary>
        /// 预测方法，根据输入的X值计算Y值。
        /// </summary>
        /// <param name="x">输入的X值。</param>
        /// <returns>预测的Y值。</returns>
        public double Predict(double x)
        {
            return Slope * x + Intercept;
        }

        /// <summary>
        /// 评估模型性能的方法，计算均方误差MSE。
        /// </summary>
        /// <param name="dataPoints">包含数据点的列表，用于评估模型性能。</param>
        /// <returns>均方误差MSE。</returns>
        public double ComputeMSE(List<(double X, double Y)> dataPoints)
        {
            double sumSquaredErrors = 0.0;
            foreach (var (X, Y) in dataPoints)
            {
                double predictedY = Predict(X);
                sumSquaredErrors += Math.Pow(predictedY - Y, 2);
            }
            return sumSquaredErrors / dataPoints.Count;
        }

        /// <summary>
        /// 主程序入口点，用于演示如何使用LinearRegression类。
        /// </summary>
        public static void Main()
        {
            // 示例数据点
            List<(double X, double Y)> dataPoints = new List<(double, double)>
            {
                (1, 2),
                (2, 3),
                (3, 5),
                (4, 6),
                (5, 7)
            };

            // 创建线性回归模型
            try
            {
                LinearRegression model = new LinearRegression(dataPoints);

                // 输出模型参数
                Console.WriteLine($"Slope: {model.Slope}, Intercept: {model.Intercept}");

                // 进行预测
                double x = 6;
                double predictedY = model.Predict(x);
                Console.WriteLine($"Predicted Y for X={x}: {predictedY}");

                // 评估模型性能
                double mse = model.ComputeMSE(dataPoints);
                Console.WriteLine($"Mean Squared Error (MSE): {mse}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }
    }
}
