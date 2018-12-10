using System.Collections.Generic;

using EasyPutty.Views;

using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMSupportContacts : TVMEasyPuttyBase {

    public IList<TVMSupportContact> Items { get; } = new List<TVMSupportContact>();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMSupportContacts() : base() { }

    public TVMSupportContacts(TSupportContactCollection contacts) : base() {
      if ( contacts == null ) {
        return;
      }
      foreach ( ISupportContact ContactItem in contacts.Items ) {
        Items.Add(new TVMSupportContact(ContactItem));
      }
    }

    public TVMSupportContacts(ISupportContactContainer contactContainer) : base() {
      if ( contactContainer == null ) {
        return;
      }
    }

    protected override void _InitializeCommands() {
    }

    protected override void _Initialize() {
    }

    public override void Dispose() {
      Items.Clear();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void DisplaySupportMessage() {
      PopupDisplaySupportContact Popup = new PopupDisplaySupportContact {
        DataContext = this
      };
      Popup.ShowDialog();
    }

    

    public static TVMSupportContacts Demo {
      get {
        if (_Demo==null) {
          TSupportContactCollection SupportContacts = new TSupportContactCollection();
          SupportContacts.Add(TSupportContact.Demo1);
          SupportContacts.Add(TSupportContact.Demo2);
          _Demo = new TVMSupportContacts(SupportContacts);
        }
        return _Demo;
      }
    }
    private static TVMSupportContacts _Demo;
  }
}
