using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20.Interfaces {
  public interface IView {
    void Show();
    void Close();

    bool? DialogResult { get; set; }
  }
}
