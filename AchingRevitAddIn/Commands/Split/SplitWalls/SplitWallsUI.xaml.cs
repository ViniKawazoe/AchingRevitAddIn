﻿#region namespaces
using System;
using System.Windows;
#endregion

namespace AchingRevitAddIn
{
    /// <summary>
    /// Interaction logic for SplitWallsUI.xaml
    /// </summary>
    public partial class SplitWallsUI : Window, IDisposable
    {
        public SplitWallsUI()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}