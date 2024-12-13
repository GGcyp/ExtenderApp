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
        //private bool interceptRequired;
        ///// <summary>
        ///// 是否需要计算截距。
        ///// </summary>
        //public bool InterceptRequired
        //{
        //    get => interceptRequired;
        //    set
        //    {
        //        InterceptAction = value ? WithIntercept : WithoutIntercept;
        //         interceptRequired = value;
        //    }
        //}

        ///// <summary>
        ///// 选择是否需要计算截距的函数
        ///// </summary>
        //protected Action InterceptAction { get; private set; }

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
        public BaseMachineLearning()
        {

        }

        /// <summary>
        /// 数据拟合方法
        /// </summary>
        /// <param name="matrixX">自变量矩阵</param>
        /// <param name="matrixY">因变量矩阵</param>
        /// <param name="needTiming">是否需要计时</param>
        public virtual void DataFit(Matrix matrixX, Matrix matrixY)
        {
            MatrixX = matrixX;
            MatrixY = matrixY;
            DataFit();
        }

        /// <summary>
        /// 抽象的数据拟合方法
        /// </summary>
        /// <remarks>
        /// 该方法应被子类实现，用于计算自变量矩阵和因变量矩阵之间的线性关系，并将结果存储在CoefficientMatrix属性中。
        /// </remarks>
        public abstract void DataFit();

        /// <summary>
        /// 使用系数矩阵对矩阵进行预测。
        /// </summary>
        /// <param name="matrix">待预测的矩阵。</returns>
        /// <returns>预测结果矩阵，维度与因变量维度相同。</returns>
        public virtual Matrix Prediction(Matrix matrix)
        {
            if (CoefficientMatrix.Row != matrix.Column && CoefficientMatrix.Row != matrix.Row)
                throw new ArgumentException(nameof(Prediction));
            else if (CoefficientMatrix.Row == matrix.Row)
                matrix = matrix.Transpose();

            return matrix.Dot(CoefficientMatrix);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string line = "--------";
            sb.AppendLine();

            sb.Append(line);
            sb.Append(GetType().Name);
            sb.Append(line);
            sb.AppendLine();

            sb.Append(line);
            sb.Append("系数");
            sb.Append(line);
            sb.AppendLine();

            sb.Append("系数个数：");
            sb.Append(CoefficientMatrix.Row.ToString());
            sb.AppendLine();

            sb.Append(CoefficientMatrix.ToString());
            sb.AppendLine();

            //sb.Append(line);
            //sb.Append("截距");
            //sb.Append(line);
            //sb.AppendLine();

            //sb.Append(Intercept.ToString());
            //sb.AppendLine();

            return sb.ToString();
        }
    }
}
