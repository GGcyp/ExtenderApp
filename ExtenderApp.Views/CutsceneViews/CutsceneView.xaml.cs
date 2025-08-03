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
using System.ComponentModel;

namespace ExtenderApp.Views.CutsceneViews
{
    /// <summary>
    /// CutsceneView.xaml 的交互逻辑
    /// </summary>
    public partial class CutsceneView : ExtenderAppView, ICutsceneView
    {
        private readonly CutsceneViewModel viewModel;
        public CutsceneView()
        {
            InitializeComponent();
            viewModel = new CutsceneViewModel();
            DataContext = viewModel;
        }

        public void Start()
        {
            Start(null);
        }

        public void Start(Action? callback)
        {
            EventHandler eventHandler = null;
            if (callback != null)
            {
                eventHandler = (o, e) => callback?.Invoke();
            }

            backgroundBehavior.ToggleVisibility(true, eventHandler);
            foregroundBehavior.ToggleVisibility(true);
        }

        public void End()
        {
            End(null);
        }

        public void End(Action? callback)
        {
            EventHandler eventHandler = null;
            if (callback != null)
            {
                eventHandler = (o, e) => callback?.Invoke();
            }
            backgroundBehavior.ToggleVisibility(false, eventHandler);
            foregroundBehavior.ToggleVisibility(false);
        }
    }
}
