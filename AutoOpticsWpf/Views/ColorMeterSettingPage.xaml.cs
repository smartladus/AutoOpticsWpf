﻿using System.Windows.Controls;
using AutoOpticsWpf.ViewModels;

namespace AutoOpticsWpf.Views
{
    /// <summary>
    /// ColorMeterSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class ColorMeterSettingPage : Page
    {
        public ColorMeterSettingPage()
        {
            InitializeComponent();
            DataContext = new ColorMeterSettingViewModel();
        }
    }
}
