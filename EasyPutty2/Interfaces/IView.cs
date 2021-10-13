using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPutty.Interfaces {
  public interface IView {
    void Show();
    bool? ShowDialog();
    void Close();

    object DataContext { get; set; }

    bool? DialogResult { get; set; }

 
  }
}
