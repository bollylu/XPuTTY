﻿using EasyPutty.Base;
using BLTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace EasyPutty {
  public class TSupportContact : TEasyPuttyBase, ISupportContact, IToXml {

    /// <summary>
    /// Set this to true to view additional debug message
    /// </summary>
    public static bool IsDebug = false;

    #region XML constants
    public static XName XML_THIS_ELEMENT {
      get {
        return GetXName("SupportContact");
      }
    }
    public const string XML_ATTRIBUTE_EMAIL = "Email";
    public const string XML_ATTRIBUTE_PHONE = "Phone";
    public const string XML_ATTRIBUTE_URI = "Url";
    public static XName XML_ELEMENT_MESSAGE {
      get {
        return GetXName("Message");
      }
    }
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

    #region Constructor(s)
    public TSupportContact() {
      Name = DEFAULT_NAME;
      Description = DEFAULT_DESCRIPTION;
      Email = DEFAULT_EMAIL;
      Phone = DEFAULT_PHONE;
      HelpUri = DEFAULT_URI;
      Message = DEFAULT_MESSAGE;
    }

    public TSupportContact(ISupportContact contact) : this() {
      Name = contact.Name;
      Description = contact.Description;
      Email = contact.Email;
      Phone = contact.Phone;
      HelpUri = contact.HelpUri;
      Message = contact.Message;
    }

    public TSupportContact(XElement contact) : this() {
      #region Validate parameters
      if (contact == null || !contact.HasAttributes) {
        Trace.WriteLineIf(IsDebug, string.Format("Unable to create {0} from XML : XElement is empty or invalid", this.GetType().Name));
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
    #endregion Constructor(s)

    #region Converters
    public override XElement ToXml() {
      XElement RetVal = base.ToXml(XML_THIS_ELEMENT);
      if (!string.IsNullOrWhiteSpace(Email)) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_EMAIL, Email);
      }
      if (!string.IsNullOrWhiteSpace(Phone)) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_PHONE, Phone);
      }
      if (!string.IsNullOrWhiteSpace(HelpUri)) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_URI, HelpUri);
      }
      if (!string.IsNullOrWhiteSpace(Message)) {
        RetVal.SetElementValue(XML_ELEMENT_MESSAGE, Message);
      }
      return RetVal;
    }

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendFormat("Support contact : {0}", Name);
      return RetVal.ToString();
    }
    #endregion Converters

    public string SupportMessage {
      get {
        StringBuilder RetVal = new StringBuilder();
        RetVal.AppendLine("Support information");
        RetVal.AppendLine();
        RetVal.AppendLine(string.Format("Email : {0}", Email));
        RetVal.AppendLine(string.Format("Phone : {0}", Phone));
        RetVal.AppendLine(string.Format("Url : {0}", HelpUri));
        RetVal.AppendLine();
        if (!string.IsNullOrWhiteSpace(Message)) {
          RetVal.AppendLine(Message);
        }
        return RetVal.ToString();
      }
    }

    public virtual void DisplaySupportMessage() {
      MessageBox.Show(SupportMessage, Description, MessageBoxButton.OK, MessageBoxImage.Information);
      return;
    }

    public string ToHtml() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendLine("<TABLE align=center style=\"border: 1px solid red\">");
      RetVal.AppendLine("<TR>");
      RetVal.AppendLine($"<TD colspan=\"3\" style=\"text-align:left; border-bottom: solid 1px gray; padding:10px; color: red; font-size:125%; font-weight: bold;\">{Description}</TD>");
      RetVal.AppendLine("</TR>");
      RetVal.AppendLine("<TR style=\"padding: 10px;\">");
      RetVal.AppendLine($"<TD style=\"width:25%;\">Tél.<br/>{Phone}</TD>");
      RetVal.AppendLine($"<TD style=\"width:50%;\">Mail<br/>{Email}</TD>");
      RetVal.AppendLine($"<TD style=\"width:25%;\">Help<br/>{HelpUri}</TD>");
      RetVal.AppendLine("</TR>");
      RetVal.AppendLine("<TR>");
      RetVal.AppendLine($"<TD colspan=\"3\" style=\"text-align:left; color: red; padding: 10px; border-top: 1px solid gray;\">{Message}</TD>");
      RetVal.AppendLine("</TR>");
      RetVal.AppendLine("</TABLE>");
      RetVal.AppendLine("<BR/>");
      return RetVal.ToString();
    }

  }
}
