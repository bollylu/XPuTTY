using System;
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

namespace XPuttyMan {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    #region Public static variables
    /// <summary>
    /// public global reference to the main window
    /// </summary>
    public static MainWindow Self;
    public static NetworkCredential CurrentUserCredential;
    public MainViewModel MainItem;
    #endregion Public static variables

    public MainWindow() {
      Self = this;
      InitializeComponent();
      MVVMBase.OnExecutionStatus += (o, e) => {
        stsBar.SetStatusLeft(e.Value);
      };

      MVVMBase.OnExecutionProgress += (o, e) => {
        stsBar.SetStatusRight(e.Message);
      };

      MainItem = new MainViewModel();
      this.DataContext = MainItem;

    }

    #region Menu
    private void MnuFileQuit_Click(object sender, RoutedEventArgs e) {
      Application.Current.Shutdown();
    }
    #endregion Menu
  }
}
