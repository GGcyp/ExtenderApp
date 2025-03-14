﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ExtenderApp.Views;

namespace ExtenderApp.Media
{
    /// <summary>
    /// PlaybackView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoView : ExtenderAppView
    {
        private readonly VideoViewModle _viewModel;

        public VideoView(VideoViewModle viewModle)
        {
            InitializeComponent();

            _viewModel = viewModle;
            _viewModel.InjectView(this);
        }
    }
}
