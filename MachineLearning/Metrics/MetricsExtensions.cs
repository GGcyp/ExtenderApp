using ExtenderApp.Data;

namespace MachineLearning
{
    public static class MetricsExtensions
    {
        /// <summary>
        /// 计算均方误差（MSE）。
        /// </summary>
        /// <param name="PredictionMatrix">预测结果矩阵。</param>
        /// <param name="TrueMatrix">真实的因变量矩阵。</param>
        /// <returns>均方误差值。</returns>
        public static double CalculateMSE(this Matrix PredictionMatrix, Matrix TrueMatrix)
        {
            if (PredictionMatrix.IsEmpty || TrueMatrix.IsEmpty
                || PredictionMatrix.Row != TrueMatrix.Row
                || PredictionMatrix.Column != TrueMatrix.Column)
            {
                throw new ArgumentException(nameof(CalculateMSE));
            }

            int n = PredictionMatrix.Row;
            double sumSquaredError = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < PredictionMatrix.Column; j++)
                {
                    double error = TrueMatrix[i, j] - PredictionMatrix[i, j];
                    sumSquaredError += error * error;
                }
            }

            return sumSquaredError / n;
        }

        /// <summary>
        /// 计算平均绝对误差（MAE）。
        /// </summary>
        /// <param name="PredictionMatrix">预测结果矩阵。</param>
        /// <param name="TrueMatrix">真实的因变量矩阵。</param>
        /// <returns>平均绝对误差值。</returns>
        public static double CalculateMAE(this Matrix PredictionMatrix, Matrix TrueMatrix)
        {
            if (PredictionMatrix.IsEmpty || TrueMatrix.IsEmpty
                || PredictionMatrix.Row != TrueMatrix.Row
                || PredictionMatrix.Column != TrueMatrix.Column)
            {
                throw new ArgumentException(nameof(CalculateMAE));
            }

            int n = PredictionMatrix.Row;
            double sumAbsoluteError = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < PredictionMatrix.Column; j++)
                {
                    double error = TrueMatrix[i, j] - PredictionMatrix[i, j];
                    sumAbsoluteError += System.Math.Abs(error);
                }
            }

            return sumAbsoluteError / n;
        }

        /// <summary>
        /// 计算均方根误差（RMSE）。
        /// </summary>
        /// <param name="PredictionMatrix">预测结果矩阵。</param>
        /// <param name="TrueMatrix">真实的因变量矩阵。</param>
        /// <returns>均方根误差值。</returns>
        public static double CalculateRMSE(this Matrix PredictionMatrix, Matrix TrueMatrix)
        {
            if (PredictionMatrix.IsEmpty || TrueMatrix.IsEmpty
                || PredictionMatrix.Row != TrueMatrix.Row
                || PredictionMatrix.Column != TrueMatrix.Column)
            {
                throw new ArgumentException(nameof(CalculateRMSE));
            }

            double mse = CalculateMSE(PredictionMatrix, TrueMatrix);
            return System.Math.Sqrt(mse);
        }

        /// <summary>
        /// 计算预测矩阵和真实矩阵之间的准确率
        /// </summary>
        /// <param name="PredictionMatrix">预测矩阵</param>
        /// <param name="TrueMatrix">真实矩阵</param>
        /// <returns>准确率，范围在0到1之间</returns>
        /// <exception cref="ArgumentException">当预测矩阵或真实矩阵为空、行数或列数不匹配时抛出</exception>
        public static double AccuracyScore(this Matrix PredictionMatrix, Matrix TrueMatrix)
        {
            if (PredictionMatrix.IsEmpty || TrueMatrix.IsEmpty
                || PredictionMatrix.Row != TrueMatrix.Row
                || PredictionMatrix.Column != TrueMatrix.Column)
            {
                throw new ArgumentException(nameof(AccuracyScore));
            }

            int trueNum = 0;
            for (int i = 0; i < PredictionMatrix.Row; i++)
            {
                for (int j = 0; j < PredictionMatrix.Column; j++)
                {
                    if (PredictionMatrix[i, j] == TrueMatrix[i, j]) trueNum++;
                }
            }

            return trueNum / (PredictionMatrix.Row * PredictionMatrix.Column);
        }
    }
}
