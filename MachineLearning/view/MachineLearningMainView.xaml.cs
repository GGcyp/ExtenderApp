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

            //// 示例数据点
            //List<(double X, double Y)> dataPoints = new List<(double, double)>
            //{
            //    (1, 2),
            //    (2, 3),
            //    (3, 5),
            //    (4, 6),
            //    (5, 7)
            //};

            //// 创建线性回归模型
            //LinearRegression model = new LinearRegression(dataPoints);

            //// 输出模型参数
            //Debug.Print($"Slope: {model.Slope}, Intercept: {model.Intercept}");

            //// 进行预测
            //double x = 6;
            //double predictedY = model.Predict(x);
            //Debug.Print($"Predicted Y for X={x}: {predictedY}");
            //DrawLineGraph();
            //DrawLineGraphWithScales();
            //            MatrixMath.Determinant(new double[,]
            //            {
            //                { 4, 2, 1, 5 },
            //                { 8, 7, 2, 10 },
            //                { 4, 8, 3, 6 },
            //                { 6, 8, 4, 9 }
            //            });

            //            MatrixMath.InvertMatrix(new double[,]
            //{
            //                { 4, 2, 1, 5 },
            //                { 8, 7, 2, 10 },
            //                { 4, 8, 3, 6 },
            //                //{ 6, 8, 4, 9 }
            //            });

            //            MatrixMath.MatrixMultiply(new double[,]
            //            {
            //                { 1, -1 },
            //                { -2, 3},
            //                { 4, -2 },
            //            }, new double[,]
            //{
            //                { 2, 1 },
            //                { 3, 4 },
            //            });

            var x = new ExtenderApp.Data.Matrix(new double[,] { { 1, -1, 1 }, { 2, 1, -1 }, { 3, -2, 6 } });
            //var y = new double[,] { { 100, 80, 256 } };
            var y = new ExtenderApp.Data.Matrix(new double[,] { { 100 }, { 80 }, { 256 } });

            //var x = new ExtenderApp.Data.Matrix(new double[,] { { 1, 1 }, { 2, -1 } });
            //var y = new ExtenderApp.Data.Matrix(new double[,] { { 14 }, { 10 } });

            //var temp = MatrixMath.Dot(MatrixMath.Inverse(MatrixMath.Dot(MatrixMath.Transpose(x), x)), MatrixMath.Transpose(x));
            //temp = MatrixMath.Dot(temp, y);
            //var temp = x.Transpose().Dot(x).Inverse().Dot(x.Transpose()).Dot(y);

            LinearRegression linear = new(true);
            linear.Exercise(x.AppendColumn(1).AppendRow(new double[4] { 1, 1, 1, 1 }), y.AppendRow(50));
            Debug.Print("截距" + " : " + linear.Intercept.ToString());
            Debug.Print("系数矩阵" + " : " + linear.CoefficientMatrix.ToString());
            linear = new(false);
            linear.Exercise(x, y);
            Debug.Print("截距" + " : " + linear.Intercept.ToString());
            Debug.Print("系数矩阵" + " : " + linear.CoefficientMatrix.ToString());
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
