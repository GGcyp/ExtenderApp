using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtenderApp.Data;

namespace MachineLearning.Linear
{
    /// <summary>
    /// 逻辑回归
    /// </summary>
    public class LogisticRegression : BaseMachineLearning
    {
        public override void DataFit()
        {
            //CoefficientMatrix=1/(1+System.Math.E.)
        }

        public override Matrix Prediction(Matrix matrix)
        {
            return base.Prediction(matrix);
        }
    }
}
