using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLTools.MVVM;

using EasyPutty.ViewModels;
using EasyPutty.Views;

using libxputty;

namespace EasyPutty.ViewModels {
  public class TVMSupportContacts : AVMEasyPuttyBase {

    public List<TVMSupportContact> Items { get; } = new List<TVMSupportContact>();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMSupportContacts() : base() { }

    public TVMSupportContacts(TSupportContactCollection contacts) : base() {
      if (contacts is null) {
        return;
      }
      foreach (ISupportContact ContactItem in contacts.Items) {
        Items.Add(new TVMSupportContact(ContactItem));
      }
    }

    public TVMSupportContacts(ISupportContactContainer contactContainer) : base() {
      if (contactContainer is null) {
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

    public static TVMSupportContacts Demo
    {
      get
      {
        if (_Demo is null) {
          TSupportContactCollection SupportContacts = new();
          SupportContacts.Add(new XSupportContact());
          SupportContacts.Add(new XSupportContact());
          _Demo = new TVMSupportContacts(SupportContacts);
        }
        return _Demo;
      }
    }
    private static TVMSupportContacts _Demo;
  }
}
