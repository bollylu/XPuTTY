using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using BLTools;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TSupportContactCollection : TPuttyBase {

    #region XML constants
    public static XName XML_THIS_ELEMENT => GetXName("SupportContacts");
    #endregion XML constants

    public IList<ISupportContact> Items { get; } = new List<ISupportContact>();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TSupportContactCollection() : base() {
      Initialize();
    }

    public TSupportContactCollection(IEnumerable<ISupportContact> contacts) : base() {
      if ( contacts == null ) {
        Initialize();
        return;
      }
      foreach ( ISupportContact ContactItem in contacts ) {
        Add(ContactItem);
      }
      Initialize();
    }

    public TSupportContactCollection(XElement contacts) {
      if ( contacts == null || !contacts.Elements(TSupportContact.XML_THIS_ELEMENT).Any() ) {
        Initialize();
        return;
      }
      foreach ( XElement ContactItem in contacts.Elements(TSupportContact.XML_THIS_ELEMENT) ) {
        Add(new TSupportContact(ContactItem));
      }
      Initialize();
    }

    public TSupportContactCollection(IEnumerable<XElement> contacts) : base() {
      if ( contacts == null ) {
        Initialize();
        return;
      }
      foreach ( XElement ContactItem in contacts ) {
        Add(new TSupportContact(ContactItem));
      }
      Initialize();
    }

    public TSupportContactCollection(TSupportContactCollection contacts) : base() {
      if ( contacts == null ) {
        Initialize();
        return;
      }
      foreach ( ISupportContact ContactItem in contacts.Items ) {
        Add(new TSupportContact(ContactItem));
      }
      Initialize();
    }

    protected void Initialize() {
    }
    public override void Dispose() {
      foreach ( ISupportContact SupportContactItem in Items ) {
        SupportContactItem.Dispose();
      }
      base.Dispose();
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void Add(ISupportContact contact) {
      Items.Add(contact);
    }

    #region Converters
    public override XElement ToXml() {
      if ( !Items.Any() ) {
        return null;
      }

      if ( Items.Count == 1 ) {
        return Items.First().ToXml();
      }

      XElement RetVal = base.ToXml(XML_THIS_ELEMENT);
      foreach ( ISupportContact ContactItem in Items ) {
        RetVal.Add(ContactItem.ToXml());
      }
      return RetVal;

    }

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Support contacts : {Items.Count} items");
      return RetVal.ToString();
    }
    #endregion Converters

    public virtual void DisplaySupportMessage() {
      StringBuilder BigMessage = new StringBuilder();
      foreach ( ISupportContact ContactItem in Items ) {
        BigMessage.AppendLine(ContactItem.SupportMessage);
        BigMessage.AppendLine();
      }
      Console.WriteLine(BigMessage.ToString());
    }

  }
}
