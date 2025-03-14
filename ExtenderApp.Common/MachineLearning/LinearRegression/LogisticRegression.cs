﻿using ExtenderApp.Data;
using ExtenderApp.Common.Math;


namespace ExtenderApp.Common.MachineLearning.Linear
{
    /// <summary>
    /// 逻辑回归
    /// </summary>
    public class LogisticRegression : GradientDescent
    {
        private const double MinValue = 0.00000001;
        private const double MaxValue = 0.99999999;

        public LogisticRegression(Matrix thate = default, double learningRate = 0.0001, int epochCount = 10000, int decayStep = 8, double decayRate = 0.99) : base(learningRate, epochCount, thate, decayStep, decayRate)
        {
        }

        public override void DataFit(Matrix matrixX, Matrix matrixY)
        {
            if (CoefficientMatrix.IsEmpty)
                CoefficientMatrix = Random.NextMatrix(matrixX.Column, matrixX.Column);
            base.DataFit(matrixX, matrixY);
        }

        public override void DataFit()
        {
            //梯度
            Matrix transpose = MatrixX.Transpose();
            Matrix gradient = default;

            //临时存储矩阵
            Matrix predValue = new Matrix(MatrixX.Row, CoefficientMatrix.Column);
            Matrix lastCoefficient = new Matrix(MatrixX.Row, CoefficientMatrix.Column);

            Func<Matrix, Matrix> selectRegression = MatrixX.Column > 2 ? Softmax : m => m.Map(Sigmoid, m);

            for (int i = 0; i < EpochCount; i++)
            {
                //计算预测值
                predValue = MatrixX.Dot(CoefficientMatrix, predValue);

                //计算预测值概率
                //predValue = predValue.Map(Sigmoid, predValue);
                predValue = selectRegression(predValue);

                lastCoefficient = predValue.CopyTo(lastCoefficient);

                //计算梯度
                gradient = transpose.Dot(predValue.Sub(MatrixY), gradient).Division(MatrixX.Row);

                //更新系数
                CoefficientMatrix = CoefficientMatrix.Sub(gradient.Multiplication(LearningRate));

                if (System.Math.Abs(Loss(CoefficientMatrix) - Loss(lastCoefficient)) < double.Epsilon)
                {
                    break;
                }

                UpdateLearningRateFixedDecay();
            }
        }

        public override Matrix Prediction(Matrix matrix)
        {
            var result = base.Prediction(matrix);

            for (int i = 0; i < result.Row; i++)
            {
                for (int j = 0; j < result.Column; j++)
                {
                    result[i, j] = Sigmoid(result[i, j]);
                }
            }

            return result;
        }

        /// <summary>
        /// 根据概率预测样本类别（二分类，以0.5为阈值）
        /// </summary>
        /// <param name="matrix">输入的矩阵。</param>
        /// <returns>返回预测结果的矩阵。</returns>
        public Matrix Predict(Matrix matrix)
        {
            var result = Prediction(matrix);
            for (int i = 0; i < result.Row; i++)
            {
                for (int j = 0; j < result.Column; j++)
                {
                    result[i, j] = result[i, j] >= 0.5 ? 1 : 0;
                }
            }

            return result;
        }

        /// <summary>
        /// 逻辑回归模型的Sigmoid函数
        /// </summary>
        /// <param name="t">输入值</param>
        /// <returns>Sigmoid函数计算结果</returns>
        public double Sigmoid(double t)
        {
            return 1.0 / (1.0 + System.Math.Exp(-t));
        }

        /// <summary>
        /// 对矩阵的每一行进行Softmax处理。
        /// </summary>
        /// <param name="matrix">需要进行Softmax处理的矩阵。</param>
        /// <returns>经过Softmax处理后的矩阵。</returns>
        private Matrix Softmax(Matrix matrix)
        {
            //分别对每一行进行逻辑回归
            for (int i = 0; i < matrix.Row; i++)
            {
                double sumExpZ = 0;
                for (int j = 0; j < matrix.Column; j++)
                {
                    matrix[i, j] = System.Math.Exp(matrix[i, j]);
                    sumExpZ += matrix[i, j];
                }
                for (int j = 0; j < matrix.Column; j++)
                {
                    matrix[i, j] = matrix[i, j] / sumExpZ;
                }
            }
            return matrix;
        }

        /// <summary>
        /// 返回矩阵的损失函数
        /// </summary>
        /// <param name="matrix">输入矩阵</param>
        /// <returns>返回损失值</returns>
        public double Loss(Matrix matrix)
        {
            double loss = 0.0;
            for (int i = 0; i < matrix.Row; i++)
            {
                for (int j = 0; j < matrix.Column; j++)
                {
                    var y = matrix[i, j];
                    var p = System.Math.Max(System.Math.Min(y, MaxValue), MinValue);
                    loss += -(y * System.Math.Log(p) + (1 - y) * System.Math.Log(1 - p));
                }
            }
            return loss;
        }
    }
}
