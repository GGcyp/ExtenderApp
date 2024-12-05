using System.Text;
using ExtenderApp.Data;
using ExtenderApp.Common.Math;

namespace MachineLearning
{
    /// <summary>
    /// 机器学习基础类
    /// </summary>
    public abstract class BaseMachineLearning
    {
        /// <summary>
        /// 获取一个值，该值指示是否需要计算截距。
        /// </summary>
        public bool InterceptRequired { get; }

        /// <summary>
        /// 获取计算得到的截距值。
        /// </summary>
        public double Intercept { get; protected set; }

        /// <summary>
        /// 训练数据集
        /// </summary>
        public Matrix MatrixX { get; protected set; }
        /// <summary>
        /// 结果集
        /// </summary>
        public Matrix MatrixY { get; protected set; }

        /// <summary>
        /// 获取计算得到的系数矩阵。
        /// </summary>
        public Matrix CoefficientMatrix { get; protected set; }

        /// <summary>
        /// 初始化 Regression 类的新实例。
        /// </summary>
        /// <param name="interceptRequired">指示是否需要计算截距。</param>
        public BaseMachineLearning(bool interceptRequired = true)
        {
            InterceptRequired = interceptRequired;
        }

        /// <summary>
        /// 数据拟合方法
        /// </summary>
        /// <param name="matrixX">自变量矩阵</param>
        /// <param name="matrixY">因变量矩阵</param>
        /// <remarks>
        /// 方法会计算自变量矩阵和因变量矩阵之间的线性关系，并将结果存储在CoefficientMatrix属性中。
        /// 如果InterceptRequired属性为true，则会计算截距并存储在Intercept属性中。
        /// </remarks>
        public virtual void DataFit(Matrix matrixX, Matrix matrixY)
        {
            MatrixX = matrixX;
            MatrixY = matrixY;
        }

        /// <summary>
        /// 使用系数矩阵对矩阵进行预测。
        /// </summary>
        /// <param name="matrix">待预测的矩阵。</returns>
        /// <returns>预测结果矩阵，维度与因变量维度相同。</returns>
        public virtual Matrix Prediction(Matrix matrix)
        {
            if (CoefficientMatrix.Row != matrix.Column)
                throw new ArgumentException(nameof(Prediction));

            return matrix.Dot(CoefficientMatrix);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string line = "--------";
            sb.Append(line);
            sb.Append("系数");
            sb.Append(line);
            sb.AppendLine();
            sb.Append("系数个数：");
            sb.Append(CoefficientMatrix.Row.ToString());
            sb.AppendLine();
            sb.Append(CoefficientMatrix.ToString());
            sb.AppendLine();
            sb.Append(line);
            sb.Append("截距");
            sb.Append(line);
            sb.AppendLine();
            sb.Append(Intercept.ToString());
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
