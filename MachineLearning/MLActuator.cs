using System.Net;
using Microsoft.ML;
using Microsoft.ML.SearchSpace;

namespace ExtenderApp.ML
{
    /// <summary>
    /// MLActuator 类表示一个机器学习执行器。
    /// </summary>
    public class MLActuator
    {
        private readonly MLContext _mlContext;

        public MLActuator()
        {
            _mlContext = new MLContext();
            var temp = _mlContext.Transforms.Categorical.OneHotEncoding("sd");
            _mlContext.BinaryClassification.Trainers.FastForest();
        }
    }
}
