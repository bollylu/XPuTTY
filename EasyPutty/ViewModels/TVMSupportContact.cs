using System.Diagnostics;
using System.Windows;
using BLTools.MVVM;
using libxputty_std20;
using libxputty_std20.Interfaces;
using EasyPutty.Views;

namespace EasyPutty.ViewModels {
  public class TVMSupportContact : MVVMBase {

    #region --- Relay commands --------------------------------------------
    public TRelayCommand NavigateToCommand { get; set; }
    public TRelayCommand MailToCommand { get; set; }
    #endregion --- Relay commands --------------------------------------------

    private ISupportContact _SupportContact;

    public Visibility UrlVisibility {
      get {
        return string.IsNullOrWhiteSpace(_SupportContact.HelpUri) ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public Visibility MessageVisibility {
      get {
        return string.IsNullOrWhiteSpace(_SupportContact.Message) ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public Visibility PhoneVisibility {
      get {
        return string.IsNullOrWhiteSpace(_SupportContact.Phone) ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public Visibility EmailVisibility {
      get {
        return string.IsNullOrWhiteSpace(_SupportContact.Email) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string Description => _SupportContact.Description;
    public string Message => _SupportContact.Message;
    public string HelpUri => _SupportContact.HelpUri;
    public string Email => _SupportContact.Email;
    public string Phone => _SupportContact.Phone;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMSupportContact() : base() {
    }

    public TVMSupportContact(ISupportContact supportContact) : base() {
      _SupportContact = supportContact;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    protected void InitializeCommands() {
      NavigateToCommand = new TRelayCommand(() => {
        Process LaunchProcess = new Process {
          StartInfo = new ProcessStartInfo(_SupportContact.HelpUri)
        };
        try {
          LaunchProcess.Start();
        } catch { }
      }
      , _ => true);

      MailToCommand = new TRelayCommand(() => {
        Process LaunchProcess = new Process();
        string MailTo = _SupportContact.Email.ToLower().StartsWith("mailto:") ? _SupportContact.Email : $"mailto:{_SupportContact.Email}";
        LaunchProcess.StartInfo = new ProcessStartInfo(MailTo);
        try {
          LaunchProcess.Start();
        } catch { }
      }
      , _ => true);
    }

    public void DisplaySupportMessage() {
      PopupDisplaySupportContact Popup = new PopupDisplaySupportContact {
        Title = _SupportContact.Description,
        DataContext = this
      };
      Popup.ShowDialog();
    }


    public static TVMSupportContact FakeSupportContact {
      get {
        if ( _FakeSupportContact == null ) {
          
          _FakeSupportContact = new TVMSupportContact(TSupportContact.Demo1);
        }
        return _FakeSupportContact;
      }
    }
    private static TVMSupportContact _FakeSupportContact;
  }
}
