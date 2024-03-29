﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

using BLTools.MVVM;

using EasyPutty.ViewModels;

namespace EasyPutty {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    #region Public static variables
    /// <summary>
    /// public global reference to the main window
    /// </summary>
    public static MainWindow Self;
    #endregion Public static variables

    public MainViewModel MainItem { get; private set; }

    public MainWindow() {
      //Self = this;
      InitializeComponent();
      AMVVM.OnExecutionStatus += (o, e) => {
        stsBar.SetStatusLeft(e.Value);
      };

      AMVVM.OnExecutionCompleted += (o, e) => {
        stsBar.SetStatusRight(e.Message);
      };

      AMVVM.OnExecutionProgress += (o, e) => {
        stsBar.SetStatusRight(e.Message);
      };

      AMVVM.OnInitProgressBar += (o, e) => {
        stsBar.ProgressBarMaxValue = e.Value;
        stsBar.ProgressBarVisibility = Visibility.Visible;
      };

      AMVVM.OnProgressBarCompleted += (o, e) => {
        stsBar.ProgressBarVisibility = Visibility.Collapsed;
      };

      AMVVM.OnProgressBarNewValue += (o, e) => {
        stsBar.ProgressBarValue = e.Value;
      };

      MainItem = new MainViewModel();
      DataContext = MainItem;

    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      if (MainItem is not null && MainItem.DataIsDirty) {
        if (MessageBox.Show("Some data are still unsaved. Do you really want to quit now ?", "Quit confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No) {
          e.Cancel = true;
        }
      }
      return;
    }
  }
}
