﻿using System;
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
using ExtenderApp.Views;

namespace ExtenderApp.Test
{
    /// <summary>
    /// TestMainView.xaml 的交互逻辑
    /// </summary>
    public partial class TestMainView : ExtenderAppView
    {
        private readonly TestMainViewModel _viewModel;

        public TestMainView(TestMainViewModel viewModel)
        {
            InitializeComponent();

            this._viewModel = viewModel;
        }
    }
}
