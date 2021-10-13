using System;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using BLTools;
using BLTools.WPF;
using EasyPutty.Interfaces;


namespace EasyPutty.Views {
  /// <summary>
  /// Interaction logic for ViewEditSession.xaml
  /// </summary>
  public partial class ViewEditSession : Window, IView, IPassword {
    public ViewEditSession() {
      InitializeComponent();
    }

    public SecureString GetPassword() {
      if ( this.IsLoaded ) {
        PasswordBox PasswordOwner = this.FindVisualChild<PasswordBox>();
        if ( PasswordOwner == null ) {
          throw new ApplicationException("Password box not defined in ViewEditSession");
        }
        Log.Write($"Getting password={PasswordOwner.SecurePassword.ConvertToUnsecureString()}");
        return PasswordOwner.SecurePassword;
      }
      return "".ConvertToSecureString();
    }

    public void SetPassword(SecureString password) {
      if ( this.IsLoaded ) {
        PasswordBox PasswordOwner = this.FindVisualChild<PasswordBox>();
        if ( PasswordOwner == null ) {
          throw new ApplicationException("Password box not defined in ViewEditSession");
        }
        PasswordOwner.Password = password.ConvertToUnsecureString();
        Log.Write($"Setting password={PasswordOwner.SecurePassword.ConvertToUnsecureString()}");
      }
    }
  }
}
