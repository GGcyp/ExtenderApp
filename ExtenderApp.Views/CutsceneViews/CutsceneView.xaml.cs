using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    /// <summary>
    /// CutsceneView.xaml 的交互逻辑
    /// </summary>
    public partial class CutsceneView : ExtenderAppView, ICutsceneView
    {
        public CutsceneView()
        {
            InitializeComponent();
        }

        public void Start()
        {
            // 初始化位置到中心
            UpdateCenteredPositions();
            // 开始进入动画
            BeginEnterAnimation();
        }

        private void UpdateCenteredPositions()
        {
            // 计算中心位置
            var centerX = AnimationCanvas.ActualWidth / 2;
            var centerY = AnimationCanvas.ActualHeight / 2;

            // 设置小球和文本的中心位置
            BallTranslate.X = centerX;
            BallTranslate.Y = centerY;
            TextTranslate.X = centerX;
            TextTranslate.Y = centerY;
        }

        /// <summary>
        /// 开始进入动画 (从小到大遮蔽)
        /// </summary>
        public void BeginEnterAnimation()
        {
            UpdateCenteredPositions();
            var storyboard = (Storyboard)Resources["EnterStoryboard"];
            storyboard.Begin();
        }

        public void End()
        {
            BeginExitAnimation();
        }

        /// <summary>
        /// 开始退出动画 (从大到小消失)
        /// </summary>
        public void BeginExitAnimation()
        {
            var storyboard = (Storyboard)Resources["ExitStoryboard"];
            storyboard.Begin();
        }
    }
}
