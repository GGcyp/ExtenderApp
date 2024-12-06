using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using ExtenderApp.Common.Math;
using ExtenderApp.Data;

namespace MachineLearning.view
{
    /// <summary>
    /// MachineLearningMainView.xaml 的交互逻辑
    /// </summary>
    public partial class MachineLearningMainView : UserControl, IView
    {
        public IViewModel ViewModel => null;
        public MachineLearningMainView()
        {
            InitializeComponent();
            //LinearRegression linear = new(true);
            //linear.CreateTestData(null, 500, 1, trueIntercept: 20, noiseStdDev: 8);
            //Debug.Print("训练集");
            //Debug.Print(linear.MatrixX.ToString());
            //Debug.Print(linear.MatrixY.ToString());
            Random random = new Random();
            var temp = random.CreateLinearRegressionTestData(500, numIndependentVariables: 10, trueIntercept: 20, noiseStdDev: 1);
            //GradientDescent gradient;
            //double rate = 0.0001;
            //int epochCount = 10000;
            //gradient = new GradientDescent_SGD(temp.Item1, temp.Item2, rate, epochCount);
            //gradient.DataFit();
            //Debug.Print(gradient.Theta.ToString());
            //gradient = new GradientDescent_MBGD(temp.Item1, temp.Item2, rate, epochCount, randomCount: 10);
            //gradient.DataFit();
            //Debug.Print(gradient.Theta.ToString());
            //gradient = new GradientDescent_BGD(temp.Item1, temp.Item2, 0.00001, epochCount);
            //gradient.DataFit(); 
            //Debug.Print(gradient.Theta.ToString());
            //var matrix = temp.Item1;
            //Debug.Print(matrix.ZScoreNormalizationRow().ToString());
            //Debug.Print("-----------------");
            //Debug.Print(matrix.ZScoreNormalizationColumn().ToString());
            //Debug.Print("-----------------");
            //Debug.Print(matrix.MinMaxNormalizationColumn().ToString());
            //Debug.Print("-----------------");
            //Debug.Print(matrix.MinMaxNormalizationRow().ToString());

            LinearRegression linear = new();
            linear.DataFit(temp.Item1, temp.Item2);
            Debug.Print(linear.ToString());
            Debug.Print("----------------");
            linear = new();
            var normX = temp.Item1.MinMaxNormalizationColumn();
            linear.DataFit(normX, temp.Item2);
            Debug.Print(linear.ToString());
            RidgeRegression ridge = new RidgeRegression();
            ridge.DataFit(temp.Item1, temp.Item2);
            Debug.Print(ridge.ToString());
            ridge.DataFit(normX, temp.Item2);
            Debug.Print(ridge.ToString());
        }

        private void DrawLineGraph()
        {
            // 定义点集
            Point[] points =
            {
                new Point(50, 150),
                new Point(150, 200),
                new Point(250, 100),
                new Point(350, 300),
                new Point(450, 250)
            };

            // 创建一个Polyline并设置其点集
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Points = new PointCollection(points)
            };

            // 将Polyline添加到Canvas中
            canvas.Children.Add(polyline);
        }

        private void DrawLineGraphWithScales()
        {
            // 定义绘图区域的大小和位置
            double graphWidth = 400;
            double graphHeight = 300;
            double leftMargin = 50;
            double bottomMargin = 50;

            // 定义刻度的间隔和数量
            double scaleInterval = 50;
            int numVerticalScales = (int)Math.Ceiling(graphHeight / scaleInterval);
            int numHorizontalScales = (int)Math.Ceiling(graphWidth / scaleInterval);

            // 创建线图点集
            Point[] points =
            {
                new Point(leftMargin + 50, bottomMargin + 250),
                new Point(leftMargin + 150, bottomMargin + 100),
                new Point(leftMargin + 250, bottomMargin + 200),
                new Point(leftMargin + 350, bottomMargin + 300)
            };

            // 绘制线图
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Points = new System.Windows.Media.PointCollection(points)
            };
            canvas.Children.Add(polyline);

            // 绘制左边的刻度
            for (int i = 0; i <= numVerticalScales; i++)
            {
                double y = bottomMargin + i * scaleInterval;
                Line scaleLine = new Line
                {
                    X1 = leftMargin - 10,
                    Y1 = y,
                    X2 = leftMargin,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(scaleLine);

                // 绘制刻度标签
                TextBlock scaleLabel = new TextBlock
                {
                    Text = $"{i * scaleInterval}",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(-30, 0, 0, 0)
                };
                Canvas.SetLeft(scaleLabel, leftMargin - 40);
                Canvas.SetTop(scaleLabel, y);
                canvas.Children.Add(scaleLabel);
            }

            // 绘制下边的刻度（水平方向）
            for (int i = 0; i <= numHorizontalScales; i++)
            {
                double x = leftMargin + i * scaleInterval;
                Line scaleLine = new Line
                {
                    X1 = x,
                    Y1 = bottomMargin,
                    X2 = x,
                    Y2 = bottomMargin - 10,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(scaleLine);

                // 绘制刻度标签
                TextBlock scaleLabel = new TextBlock
                {
                    Text = i.ToString(CultureInfo.InvariantCulture),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(scaleLabel, x);
                Canvas.SetTop(scaleLabel, bottomMargin - 20);
                canvas.Children.Add(scaleLabel);
            }
        }

        public void Enter(IView oldView)
        {

        }

        public void Exit(IView newView)
        {

        }
    }
}
