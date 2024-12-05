using ExtenderApp.Common.Math;
using ExtenderApp.Data;
using HandyControl.Themes;

namespace MachineLearning
{
    /// <summary>
    /// 梯度下降线性回归类
    /// </summary>
    public abstract class GradientDescent : BaseMachineLearning
    {
        /// <summary>
        /// 静态的随机数生成器实例
        /// </summary>
        private static Random random;

        /// <summary>
        /// 获取随机数生成器实例
        /// </summary>
        protected static Random Random
        {
            get
            {
                if (random is null)
                {
                    random = new Random();
                }
                return random;
            }
        }

        /// <summary>
        /// 学习率
        /// </summary>
        public double LearningRate { get; set; }

        /// <summary>
        /// 初始学习率
        /// </summary>
        protected double InitialLearningRate { get; set; }

        /// <summary>
        /// 学习次数
        /// </summary>
        public int EpochCount { get; set; }

        /// <summary>
        /// 衰减步长，每经过这么多次迭代进行一次衰减
        /// </summary>
        public int DecayStep { get; set; }

        /// <summary>
        /// 衰减比例，每次衰减时学习率乘以该比例
        /// </summary>
        public double DecayRate { get; set; }

        /// <summary>
        /// 迭代次数
        /// </summary>
        protected int Iteration { get; set; }

        /// <summary>
        /// 当前衰减步数
        /// </summary>
        private int currentDecayStep;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="matrixX">X矩阵</param>
        /// <param name="matrixY">Y矩阵</param>
        /// <param name="learningRate">学习率</param>
        /// <param name="epochCount">学习次数</param>
        /// <param name="theta">求解系数，默认为null</param>
        public GradientDescent(double learningRate, int epochCount, int decayStep = 8, double decayRate = 0.99)
        {
            LearningRate = learningRate;
            InitialLearningRate = learningRate;
            EpochCount = epochCount;
            Iteration = 0;
            DecayRate = decayRate;
            DecayStep = epochCount / decayStep;
            currentDecayStep = decayStep;
        }

        public override void DataFit(Matrix matrixX, Matrix matrixY)
        {
            base.DataFit(matrixX, matrixY);
            CoefficientMatrix = Random.NextMatrix(MatrixX.Column, 1);
        }

        /// <summary>
        /// 根据固定步长和衰减比例更新学习率（固定步长衰减）
        /// </summary>
        protected void UpdateLearningRateFixedDecay()
        {
            Iteration++;
            if (Iteration - currentDecayStep == 0)
            {
                LearningRate *= DecayRate;
                currentDecayStep += DecayStep;
            }
        }

        /// <summary>
        /// 根据指数衰减规则更新学习率
        /// </summary>
        protected void UpdateLearningRateExponentialDecay()
        {
            Iteration++;
            LearningRate = InitialLearningRate * System.Math.Pow(DecayRate, Iteration);
        }

    }
}
