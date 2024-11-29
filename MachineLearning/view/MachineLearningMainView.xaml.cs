using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Abstract;

namespace MachineLearning.view
{
    /// <summary>
    /// MachineLearningMainView.xaml 的交互逻辑
    /// </summary>
    public partial class MachineLearningMainView : UserControl,IView
    {
        public IViewModel ViewModel => null;
        public MachineLearningMainView()
        {
            InitializeComponent();

            // 示例数据点
            List<(double X, double Y)> dataPoints = new List<(double, double)>
            {
                (1, 2),
                (2, 3),
                (3, 5),
                (4, 6),
                (5, 7)
            };

            // 创建线性回归模型
            LinearRegression model = new LinearRegression(dataPoints);

            // 输出模型参数
            Debug.Print($"Slope: {model.Slope}, Intercept: {model.Intercept}");

            // 进行预测
            double x = 6;
            double predictedY = model.Predict(x);
            Debug.Print($"Predicted Y for X={x}: {predictedY}");
        }


        public void Enter(IView oldView)
        {
            
        }

        public void Exit(IView newView)
        {
            
        }
    }
}
