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
using EasyPutty.ViewModels;

namespace EasyPutty.Views {
  /// <summary>
  /// Interaction logic for ViewSessionsList.xaml
  /// </summary>
  public partial class ViewSessionsList : UserControl {
    public ViewSessionsList() {
      InitializeComponent();
    }


    private void DataTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      foreach ( VMPuttySession SessionItem in e.AddedItems ) {
        SessionItem.IsSelected = true;
      }
      foreach ( VMPuttySession SessionItem in e.RemovedItems ) {
        SessionItem.IsSelected = false;
      }
      e.Handled = true;
    }
  }
}
