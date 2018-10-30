using EasyPutty.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace ServicesManager {
  public class TSupportContactWPF : TSupportContact, ISupportContact {

    public Visibility UrlVisibility {
      get {
        return string.IsNullOrWhiteSpace(HelpUri) ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public Visibility MessageVisibility {
      get {
        return string.IsNullOrWhiteSpace(Message) ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public Visibility PhoneVisibility {
      get {
        return string.IsNullOrWhiteSpace(Phone) ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public Visibility EmailVisibility {
      get {
        return string.IsNullOrWhiteSpace(Email) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public TSupportContactWPF() : base() {
      InitializeCommands();
    }
    public TSupportContactWPF(XElement supportContact) : base(supportContact) {
      InitializeCommands();
    }
    public TSupportContactWPF(ISupportContact supportContact) {
      InitializeCommands();
      Name = supportContact.Name;
      Description = supportContact.Description;
      Email = supportContact.Email;
      Phone = supportContact.Phone;
      HelpUri = supportContact.HelpUri;
      Message = supportContact.Message;
    }
    private void InitializeCommands() {
      NavigateToCommand = new TRelayCommand(() => {
        Process LaunchProcess = new Process();
        LaunchProcess.StartInfo = new ProcessStartInfo(this.HelpUri);
        try {
          LaunchProcess.Start();
        } catch { }
      }
      , _ => true);
      MailToCommand = new TRelayCommand(() => {
        Process LaunchProcess = new Process();
        string MailTo = Email.ToLower().StartsWith("mailto:") ? Email : $"mailto:{Email}";
        LaunchProcess.StartInfo = new ProcessStartInfo(MailTo);
        try {
          LaunchProcess.Start();
        } catch { }
      }
      , _ => true);
    }

    public override void DisplaySupportMessage() {
      PopupDisplaySupportContact Popup = new PopupDisplaySupportContact();
      Popup.Title = this.Description;
      Popup.DataContext = this;
      Popup.ShowDialog();
    }

    public TRelayCommand NavigateToCommand { get; set; }
    public TRelayCommand MailToCommand { get; set; }

  }
}
