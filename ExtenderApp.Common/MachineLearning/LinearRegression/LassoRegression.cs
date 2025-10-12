

using System.Numerics;
using ExtenderApp.Common.Math;
using ExtenderApp.Data;

namespace ExtenderApp.Common.MachineLearning.Linear
{
    public class LassoRegression : BaseMachineLearning
    {
        public double Lambda { get; set; } // 正则化参数lambda
        public int MaxIterations { get; set; } // 最大迭代次数
        public double Tolerance { get; set; } // 收敛容忍度

        private int updateIndex;
        /// <summary>
        /// 当前更新系数矩阵
        /// </summary>
        private Matrix currentCoefficientMatrix;
        /// <summary>
        /// 上次的系数矩阵
        /// </summary>
        private Matrix prevCoefficientMatrix;
        /// <summary>
        /// 残差矩阵
        /// </summary>
        private Matrix residualMatrix;
        /// <summary>
        /// 单一系数矩阵
        /// </summary>
        private Matrix singleCoefficientMatrix;
        /// <summary>
        /// 单一系数矩阵点乘净残值矩阵（临时矩阵）
        /// </summary>
        private Matrix singleDotResidualMatrix;
        /// <summary>
        /// 单一系数转置矩阵（临时矩阵）
        /// </summary>
        private Matrix singleCoefficientTransposeMatrix;
        /// <summary>
        /// 单一系数转置矩阵点乘单一系数矩阵（临时矩阵）
        /// </summary>
        private Matrix singleCoefficientTransposeDotsingleCoefficientMatrix;

        public LassoRegression(double lambda, int maxIterations=1000, double tolerance= 1e-6)
        {
            Lambda = lambda;
            MaxIterations = maxIterations;
            Tolerance = tolerance;
        }

        public override void DataFit()
        {
            updateIndex = 0;

            CreateMatrix();

            for (int i = 0; i < MaxIterations; i++)
            {
                ////Vector<Float64> prevBeta = beta.Clone();
                for (int j = 0; j < MatrixX.Column; j++)
                {
                    UpdateCoefficient();
                }

                // 计算系数向量的变化量（使用L2范数衡量）
                double change = (currentCoefficientMatrix - prevCoefficientMatrix).TwoNorm();
                if (change < Tolerance)
                {
                    break;
                }

                currentCoefficientMatrix.CopyTo(prevCoefficientMatrix);
            }

            CoefficientMatrix = currentCoefficientMatrix;
        }

        /// <summary>
        /// 创建临时矩阵
        /// </summary>
        private void CreateMatrix()
        {
            //if (CoefficientMatrix.IsEmpty || CoefficientMatrix.Row != MatrixX.Column)
            //    CoefficientMatrix = new(MatrixX.Column, 1);

            if (currentCoefficientMatrix.IsEmpty || currentCoefficientMatrix.Row != MatrixX.Row)
                currentCoefficientMatrix = new(1, MatrixX.Column);

            if (prevCoefficientMatrix.IsEmpty || prevCoefficientMatrix.Row != MatrixX.Row)
                prevCoefficientMatrix = new(1, MatrixX.Column);

            if (residualMatrix.IsEmpty || residualMatrix.Row != MatrixX.Row)
                residualMatrix = new(MatrixX.Row, 1);

            if (singleCoefficientMatrix.IsEmpty || singleCoefficientMatrix.Row != MatrixX.Row)
                singleCoefficientMatrix = new(MatrixX.Row, 1);

            if (singleCoefficientTransposeMatrix.IsEmpty || singleCoefficientTransposeMatrix.Row != MatrixX.Row)
                singleCoefficientTransposeMatrix = new(MatrixX.Row, 1);

            if (singleCoefficientTransposeMatrix.IsEmpty || singleCoefficientTransposeMatrix.Row != MatrixX.Row)
                singleCoefficientTransposeMatrix = new(MatrixX.Row, 1);

            if (singleDotResidualMatrix.IsEmpty || singleDotResidualMatrix.Row != MatrixX.Row)
                singleDotResidualMatrix = new(1, 1);
        }

        /// <summary>
        /// 使用坐标下降法更新单个系数（软阈值操作）。
        /// 在每次迭代中，针对每个系数，根据当前其他系数的值以及数据计算更新该系数的值，使其朝着使损失函数减小的方向变化。
        /// </summary>
        private void UpdateCoefficient()
        {
            int n = MatrixX.Row; // 样本数量，假设Matrix类有Row属性获取行数
            for (int i = 0; i < n; i++)
            {
                singleCoefficientMatrix[updateIndex, 0] = MatrixX[i, updateIndex]; // 假设Matrix类可以通过[i, j]索引器获取元素
            }

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int k = 0; k < MatrixX.Column; k++)
                {
                    if (k != updateIndex)
                    {
                        sum += currentCoefficientMatrix[0, k] * MatrixX[i, k];
                    }
                }
                residualMatrix[i, 0] = MatrixY[i, 0] - sum;
            }

            singleCoefficientTransposeMatrix = singleCoefficientMatrix.Transpose(singleCoefficientTransposeMatrix);
            //单一系数转置矩阵点乘净残值矩阵
            singleDotResidualMatrix = singleCoefficientTransposeMatrix.Dot(residualMatrix, singleDotResidualMatrix);
            //单一系数矩阵点乘净残值矩阵除于（单一系数转置矩阵点单一系数矩阵）
            singleDotResidualMatrix = singleDotResidualMatrix.Division(singleCoefficientTransposeMatrix.Dot(singleCoefficientMatrix, singleCoefficientTransposeDotsingleCoefficientMatrix));


            //如果绝对值大于threshold，则根据正负返回减去或加上threshold的值；
            //若绝对值小于等于threshold，则返回 0，以此实现系数的收缩以及可能将系数变为 0 的操作，符合 Lasso 回归的特性。
            double threshold = Lambda / (2.0 * MatrixX.Row);
            double x = singleDotResidualMatrix[0, 0];
            double currentCoefficient = (System.Math.Abs(x) > threshold) ? x > 0 ? x - threshold : x + threshold : 0;
            currentCoefficientMatrix[0, updateIndex] = currentCoefficient;
        }
    }
}
