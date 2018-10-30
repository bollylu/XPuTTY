using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServicesManager {
  public class TVMSupportContact : TVMBase, ISupportContact {

    //public override ISupportContact SupportContactData {
    //  get {
    //    if (_Data is ISupportContact) {
    //      return _Data as ISupportContact;
    //    }
    //    return null;
    //  }
    //}

    public string SupportMessage {
      get {
        if (SupportContact != null) {
          return SupportContact.SupportMessage;
        }
        return "";
      }
    }

    #region Constructor(s)
    public TVMSupportContact() {

    }

    public TVMSupportContact(ISupportContact supportContact) {
      _Data = supportContact;

    } 
    #endregion Constructor(s)

    public void DisplaySupportMessage() {
      if (SupportContact != null) {
        SupportContact.DisplaySupportMessage();
      }
    }

    public override string Picture {
      get {
        return App.GetPictureFullname("help");
      }
    }

    public ISupportContact SupportContact {
      get {
        throw new NotImplementedException();
      }
    }
  }
}
