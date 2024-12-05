
namespace ExtenderApp.Common.Math
{
    public static class DataNormalizerExtensions
    {
        /// <summary>
        /// 对给定的数据数组进行最小-最大归一化处理
        /// </summary>
        /// <param name="data">需要进行归一化处理的数据数组</param>
        /// <param name="result">归一化后的结果数组，如果为null，则自动创建一个新的数组来存储结果</param>
        /// <returns>归一化后的数据数组</returns>
        /// <exception cref="ArgumentException">如果输入数据为空或长度为0，则抛出异常</exception>
        public static double[] MinMaxNormalization(this double[] data, double[] result = null)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("输入数据不能为空或长度为0", "data");
            }

            double min = data[0];
            double max = data[0];

            // 找到数据中的最小值和最大值
            for (int i = 1; i < data.Length; i++)
            {
                double num = data[i];
                if (num < min)
                {
                    min = num;
                }
                else if (num > max)
                {
                    max = num;
                }
            }

            //计算归一化
            if (result == null || result.Length != data.Length)
                result = new double[data.Length];


            double diff = max - min;
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (data[i] - min) / diff;
            }

            return result;
        }

        /// <summary>
        /// 对输入数据进行Z-Score标准化处理
        /// </summary>
        /// <param name="data">需要标准化的数据数组</param>
        /// <param name="result">存储结果的数组，如果为null，则自动创建新数组</param>
        /// <returns>返回标准化后的数据数组</returns>
        /// <exception cref="ArgumentException">当输入数据为空或长度为0时抛出异常</exception>
        public static double[] ZScoreNormalization(this double[] data, double[] result = null)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("输入数据不能为空或长度为0", "data");
            }

            double sum = 0;
            // 计算数据的总和
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

            double mean = sum / data.Length;

            double sumSquaredDiff = 0;
            // 计算数据与均值的差的平方和
            for (int i = 0; i < data.Length; i++)
            {
                double diff = data[i] - mean;
                sumSquaredDiff += diff * diff;
            }

            double stdDeviation = System.Math.Sqrt(sumSquaredDiff / data.Length);

            //计算归一化
            if (result is null || result.Length != data.Length)
                result = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (data[i] - mean) / stdDeviation;
            }

            return result;
        }
    }
}
