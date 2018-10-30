using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools.MVVM;
using EasyPutty.ViewModels;
using EasyPutty.Views;
using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty {
  public class TVMSupportContacts : MVVMBase {

    private List<TVMSupportContact> _SupportContacts { get; } = new List<TVMSupportContact>();


    public TVMSupportContacts() : base() { }

    public TVMSupportContacts(TSupportContactCollection contacts) : this() {
      if ( contacts == null ) {
        return;
      }

      foreach ( ISupportContact ContactItem in contacts.Items ) {
        _SupportContacts.Add(new TVMSupportContact(ContactItem));
      }
    }

    public TVMSupportContacts(ISupportContactContainer contactContainer) : this() {
      if ( contactContainer == null ) {
        return;
      }

    }

    public void DisplaySupportMessage() {
      PopupDisplaySupportContact Popup = new PopupDisplaySupportContact {
        DataContext = this
      };
      Popup.ShowDialog();
    }

  }
}
