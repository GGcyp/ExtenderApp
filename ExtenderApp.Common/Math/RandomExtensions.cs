namespace ExtenderApp.Common.Math
{
    public static class RandomExtensions
    {
        /// <summary>
        /// 生成服从高斯分布（正态分布）的随机数。
        /// </summary>
        /// <param name="random">随机数生成器。</param>
        /// <param name="mean">均值。</param>
        /// <param name="standardDeviation">标准差。</param>
        /// <returns>生成的随机数。</returns>
        public static double NextGaussian(this Random random, double mean = 0, double standardDeviation = 1)
        {
            if (mean == 0 && standardDeviation == 0) return 0;

            double u1, u2, s, z;
            do
            {
                u1 = random.NextDouble();
                u2 = random.NextDouble();
                s = u1 * u1 + u2 * u2;
            } while (s >= 1 || s == 0);

            s = System.Math.Sqrt(-2 * System.Math.Log(s) / s);
            z = u1 * s;

            return mean + standardDeviation * z;
        }

        /// <summary>
        /// 生成指定数量的随机数列表。
        /// </summary>
        /// <param name="random">随机数生成器。</param>
        /// <param name="start">随机数范围起始值（包含）。</param>
        /// <param name="end">随机数范围结束值（包含）。</param>
        /// <param name="count">要生成的随机数数量。</param>
        /// <param name="result">存储结果的列表，如果为null则创建一个新的列表。</param>
        /// <returns>包含随机数的列表，如果生成的随机数数量超过范围则返回null。</returns>
        public static List<int> NextList(this Random random, int start, int end, int count, List<int> result = null)
        {
            if (count > (end - start + 1))
                return null;

            if (random is null)
                random = new Random();

            if (result != null)
                result = new(count);

            for (int i = 0; i < count; i++)
            {
                result[i] = random.Next();
            }

            return result;
        }

        /// <summary>
        /// 生成指定数量的唯一随机数列表。
        /// </summary>
        /// <param name="random">随机数生成器。</param>
        /// <param name="start">随机数范围起始值（包含）。</param>
        /// <param name="end">随机数范围结束值（包含）。</param>
        /// <param name="count">要生成的随机数数量。</param>
        /// <param name="uniqueNumbers">存储唯一随机数的集合，如果为null则创建一个新的集合。</param>
        /// <param name="result">存储结果的列表，如果为null则创建一个新的列表。</param>
        /// <returns>包含唯一随机数的列表，如果生成的随机数数量超过范围则返回null。</returns>
        public static List<int> NextExclusiveList(this Random random, int start, int end, int count, HashSet<int> uniqueNumbers = null, List<int> result = null)
        {
            if (count > (end - start + 1))
                return null;

            if (random is null)
                random = new Random();

            if (result is null)
                result = new(count);
            else
                result.Clear();

            uniqueNumbers = random.NextHashSet(start, end, count, uniqueNumbers);

            foreach (int i in uniqueNumbers)
            {
                result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// 生成指定数量的唯一随机数集合。
        /// </summary>
        /// <param name="random">随机数生成器。</param>
        /// <param name="start">随机数范围起始值（包含）。</param>
        /// <param name="end">随机数范围结束值（包含）。</param>
        /// <param name="count">要生成的随机数数量。</param>
        /// <param name="uniqueNumbers">存储唯一随机数的集合，如果为null则创建一个新的集合。</param>
        /// <returns>包含唯一随机数的集合。</returns>
        public static HashSet<int> NextHashSet(this Random random, int start, int end, int count, HashSet<int> uniqueNumbers = null)
        {
            if (random is null)
                random = new Random();

            if (uniqueNumbers is null)
                uniqueNumbers = new HashSet<int>(count);

            while (uniqueNumbers.Count < count)
            {
                int randomNumber = random.Next(start, end + 1);
                uniqueNumbers.Add(randomNumber);
            }

            return uniqueNumbers;
        }
    }
}
