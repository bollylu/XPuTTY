using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace libxputty_std20.DllImports {
  public static class User32dll {
    [DllImport("user32.dll")]
    public static extern int SetWindowText(IntPtr hWnd, string windowName);
  }
}
