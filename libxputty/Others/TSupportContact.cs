﻿using System;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using BLTools;

namespace libxputty {
  public class TSupportContact : ASessionBase, ISupportContact {

    #region XML constants
    public static XName XML_THIS_ELEMENT => GetXName("SupportContact");
    public const string XML_ATTRIBUTE_EMAIL = "Email";
    public const string XML_ATTRIBUTE_PHONE = "Phone";
    public const string XML_ATTRIBUTE_URI = "Url";
    public static XName XML_ELEMENT_MESSAGE => GetXName("Message");
    #endregion XML constants

    #region Default values
    private const string DEFAULT_NAME = "Default";
    private const string DEFAULT_DESCRIPTION = "";
    private const string DEFAULT_EMAIL = "";
    private const string DEFAULT_PHONE = "";
    private const string DEFAULT_URI = "";
    private const string DEFAULT_MESSAGE = "";
    #endregion Default values

    #region Public properties
    public string Email { get; set; }
    public string Phone { get; set; }
    public string HelpUri { get; set; }
    public string Message { get; set; }
    #endregion Public properties

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TSupportContact() : base() { }

    public TSupportContact(ISupportContact contact) : base() {
      Name = contact.Name;
      Description = contact.Description;
      Email = contact.Email;
      Phone = contact.Phone;
      HelpUri = contact.HelpUri;
      Message = contact.Message;
    }

    public TSupportContact(XElement contact) : base() {
      FromXml(contact);
    }

    protected override void _Initialize() {
      Name = DEFAULT_NAME;
      Description = DEFAULT_DESCRIPTION;
      Email = DEFAULT_EMAIL;
      Phone = DEFAULT_PHONE;
      HelpUri = DEFAULT_URI;
      Message = DEFAULT_MESSAGE;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region Converters
    public override XElement ToXml() {
      XElement RetVal = base.ToXml(XML_THIS_ELEMENT);
      if ( !string.IsNullOrWhiteSpace(Email) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_EMAIL, Email);
      }
      if ( !string.IsNullOrWhiteSpace(Phone) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_PHONE, Phone);
      }
      if ( !string.IsNullOrWhiteSpace(HelpUri) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_URI, HelpUri);
      }
      if ( !string.IsNullOrWhiteSpace(Message) ) {
        RetVal.SetElementValue(XML_ELEMENT_MESSAGE, Message);
      }
      return RetVal;
    }
    public override void FromXml(XElement contact) {
      #region Validate parameters
      if (contact == null || !contact.HasAttributes) {
        Trace.WriteLine($"Unable to create {this.GetType().Name} from XML : XElement is empty or invalid");
        return;
      }
      #endregion Validate parameters
      Name = contact.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, DEFAULT_NAME);
      Description = contact.SafeReadAttribute<string>(XML_ATTRIBUTE_DESCRIPTION, DEFAULT_DESCRIPTION);
      Email = contact.SafeReadAttribute<string>(XML_ATTRIBUTE_EMAIL, DEFAULT_EMAIL);
      Phone = contact.SafeReadAttribute<string>(XML_ATTRIBUTE_PHONE, DEFAULT_PHONE);
      HelpUri = contact.SafeReadAttribute<string>(XML_ATTRIBUTE_URI, DEFAULT_URI);
      Message = contact.SafeReadElementValue<string>(XML_ELEMENT_MESSAGE, DEFAULT_MESSAGE);
    }

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Support contact : {Name}");
      RetVal.Append($", {Message ?? ""}");
      return RetVal.ToString();
    }
    #endregion Converters

    public string SupportMessage {
      get {
        StringBuilder RetVal = new();
        RetVal.AppendLine("Support information");
        RetVal.AppendLine();
        RetVal.AppendLine($"Email : {Email ?? ""}");
        RetVal.AppendLine($"Phone : {Phone ?? ""}");
        RetVal.AppendLine($"Url : {HelpUri ?? ""}");
        RetVal.AppendLine();
        if ( !string.IsNullOrWhiteSpace(Message) ) {
          RetVal.AppendLine(Message);
        }
        return RetVal.ToString();
      }
    }

    public virtual void DisplaySupportMessage() {
      Console.WriteLine(SupportMessage);
      Console.WriteLine(Description);
      return;
    }

    public string ToHtml() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendLine("<TABLE align=center style=\"border: 1px solid red\">");
      RetVal.AppendLine("<TR>");
      RetVal.AppendLine($"<TD colspan=\"3\" style=\"text-align:left; border-bottom: solid 1px gray; padding:10px; color: red; font-size:125%; font-weight: bold;\">{Description}</TD>");
      RetVal.AppendLine("</TR>");
      RetVal.AppendLine("<TR style=\"padding: 10px;\">");
      RetVal.AppendLine($"<TD style=\"width:25%;\">Tél.<br/>{Phone ?? ""}</TD>");
      RetVal.AppendLine($"<TD style=\"width:50%;\">Mail<br/>{Email ?? ""}</TD>");
      RetVal.AppendLine($"<TD style=\"width:25%;\">Help<br/>{HelpUri ?? ""}</TD>");
      RetVal.AppendLine("</TR>");
      RetVal.AppendLine("<TR>");
      RetVal.AppendLine($"<TD colspan=\"3\" style=\"text-align:left; color: red; padding: 10px; border-top: 1px solid gray;\">{Message ?? ""}</TD>");
      RetVal.AppendLine("</TR>");
      RetVal.AppendLine("</TABLE>");
      RetVal.AppendLine("<BR/>");
      return RetVal.ToString();
    }
  }
}
