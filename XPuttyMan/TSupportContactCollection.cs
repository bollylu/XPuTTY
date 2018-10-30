using EasyPutty.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace EasyPutty {
  public class TSupportContactCollection : TEasyPuttyBase {

    #region XML constants
    public static XName XML_THIS_ELEMENT {
      get {
        return GetXName("SupportContacts");
      }
    }
    #endregion XML constants

    public IList<ISupportContact> Items {
      get {
        return _Items;
      }
    }
    private readonly IList<ISupportContact> _Items = new List<ISupportContact>();

    #region Constructor(s)
    public TSupportContactCollection() : base() {
      Initialize();
    }
    public TSupportContactCollection(IEnumerable<ISupportContact> contacts) : base() {
      if (contacts == null) {
        Initialize();
        return;
      }
      foreach (ISupportContact ContactItem in contacts) {
        AddItem(new TSupportContact(ContactItem));
      }
      Initialize();
    }
    public TSupportContactCollection(XElement contacts) : base(contacts) {
      if (contacts == null || contacts.Elements(TSupportContact.XML_THIS_ELEMENT).Count() == 0) {
        Initialize();
        return;
      }
      foreach (XElement ContactItem in contacts.Elements(TSupportContact.XML_THIS_ELEMENT)) {
        AddItem(new TSupportContact(ContactItem));
      }
      Initialize();
    }
    public TSupportContactCollection(IEnumerable<XElement> contacts) : base() {
      if (contacts == null) {
        Initialize();
        return;
      }
      foreach (XElement ContactItem in contacts) {
        AddItem(new TSupportContact(ContactItem));
      }
      Initialize();
    }
    public TSupportContactCollection(TSupportContactCollection contacts) : base() {
      if (contacts == null) {
        Initialize();
        return;
      }
      Name = contacts.Name;
      Description = contacts.Description;
      foreach (ISupportContact ContactItem in contacts.Items) {
        AddItem(new TSupportContact(ContactItem));
      }
      Initialize();
    }

    protected override void Initialize() {
      base.Initialize();
    }
    #endregion Constructor(s)

    private void AddItem(ISupportContact contact) {
      Items.Add(contact);
    }

    #region Converters
    public override XElement ToXml() {

      if (Items.Count > 0) {

        if (Items.Count == 1) {
          ISupportContact LoneItem = Items.First();
          if (LoneItem is TSupportContact) {
            TSupportContact SupportContact = LoneItem as TSupportContact;
            return SupportContact.ToXml();
          }
          return null;
        } else {
          XElement RetVal = base.ToXml(XML_THIS_ELEMENT);
          foreach (ISupportContact ContactItem in Items) {
            if (ContactItem is TSupportContact) {
              TSupportContact SupportContact = ContactItem as TSupportContact;
              RetVal.Add(SupportContact.ToXml());
            }
          }
          return RetVal;
        }
      } else {
        return null;
      }


    }
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append(string.Format($"{Items.Count} items"));
      return RetVal.ToString();
    }
    #endregion Converters

    public virtual void DisplaySupportMessage() {
      StringBuilder BigMessage = new StringBuilder();
      foreach (ISupportContact ContactItem in Items) {
        BigMessage.AppendLine(ContactItem.SupportMessage);
        BigMessage.AppendLine();
      }
      MessageBox.Show(BigMessage.ToString(), Description, MessageBoxButton.OK, MessageBoxImage.Information);
    }
  }
}
