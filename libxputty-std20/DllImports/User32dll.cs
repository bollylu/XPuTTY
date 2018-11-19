using System;
using System.Runtime.InteropServices;

namespace libxputty_std20.DllImports {
  public static class User32dll {

    private const string USER32 = "user32.dll";

    [DllImport(USER32)]
    public static extern int SetWindowText(IntPtr hWnd, string windowName);

    [DllImport(USER32)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  }
}
