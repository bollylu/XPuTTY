using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesManager {
  public class TVMSupportContacts {

    private TSupportContactCollection _ContactsData { get; set; }


    public IList<TSupportContactWPF> Items {
      get {
        return _Items;
      }
    }
    private readonly IList<TSupportContactWPF> _Items;

    public TVMSupportContacts() {
      _Items = new List<TSupportContactWPF>();
    }

    public TVMSupportContacts(TSupportContactCollection contacts) : this() {
      if (contacts == null) {
        return;
      }
      _ContactsData = contacts;
      foreach (ISupportContact ContactItem in contacts.Items) {
        Items.Add(new TSupportContactWPF(ContactItem));
      }
    }

    public TVMSupportContacts(ISupportContactContainer contactContainer) : this() {
      if (contactContainer == null) {
        return;
      }
      _ContactsData = contactContainer.SupportContacts;
      foreach (ISupportContact ContactItem in contactContainer.SupportContacts.Items) {
        Items.Add(new TSupportContactWPF(ContactItem));
      }
    }

    public void DisplaySupportMessage() {
      PopupDisplaySupportContact Popup = new PopupDisplaySupportContact();
      Popup.DataContext = this;
      Popup.ShowDialog();
    }

  }
}
