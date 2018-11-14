﻿using System;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Linq;

using BLTools;
using BLTools.Encryption;

using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TCredential : TPuttyBase, IDisposable, ICredential {

    #region XML contants
    public static XName XML_THIS_ELEMENT => GetXName("Credential");
    public const string XML_ATTRIBUTE_USERNAME = "Username";
    public const string XML_ATTRIBUTE_XMLSECURE = "Secure";
    public const string XML_ATTRIBUTE_PASSWORD = "Password";
    public const string XML_ATTRIBUTE_INHERITED = "Inherited";
    #endregion XML contants

    #region Public properties
    public string Username { get; protected set; }

    public string Domain {
      get {
        if ( string.IsNullOrWhiteSpace(Username) ) {
          return "";
        }
        if ( !Username.Contains(@"\") && !Username.Contains("@") ) {
          return Username;
        }
        if ( Username.Contains(@"\") ) {
          return Username.Before(@"\");
        }
        return Username.After("@");
      }
    }
    public string UsernameWithoutDomain {
      get {
        if ( string.IsNullOrWhiteSpace(Username) ) {
          return "";
        }
        if ( !Username.Contains(@"\") && !Username.Contains("@") ) {
          return "";
        }
        if ( Username.Contains(@"\") ) {
          return Username.After(@"\");
        }
        return Username.Before("@");
      }
    }
    
    public bool XmlSecure {
      get {
        if ( Inherited ) {
          return true;
        }
        return _XmlSecure;
      }
      set {
        _XmlSecure = value;
      }
    }
    private bool _XmlSecure;

    public SecureString SecurePassword { get; protected set; }

    public string EncryptionKey {
      private get {
        if ( string.IsNullOrWhiteSpace(_EncryptionKey) ) {
          _EncryptionKey = MakeKey(ParentName);
        }
        return _EncryptionKey;
      }
      set {
        if ( !string.IsNullOrWhiteSpace(value) ) {
          _EncryptionKey = MakeKey(value);
        } else {
          _EncryptionKey = MakeKey(ParentName);
        }
      }
    }
    private string _EncryptionKey = "";

    public bool HasValue => !string.IsNullOrWhiteSpace(Username);
    public bool Inherited {
      get {

        if ( HasValue ) {
          return false;
        }

        if ( Parent == null ) {
          return false;
        }

        if ( !(Parent.Parent is ICredentialContainer ParentContainer) ) {
          return false;
        }

        if ( ParentContainer.Credential.HasValue ) {
          Log.Write($"Credential is coming from Parent => {ParentContainer.GetType().Name}:{ParentName}");
          return true;
        }

        return ParentContainer.Credential.Inherited;
      }
    }

    public string ParentName {
      get {
        if ( Parent is IName ParentWithName ) {
          return ParentWithName.Name;
        }
        return "";
      }
    }

    //public PSCredential PsCredential {
    //  get {
    //    if (string.IsNullOrWhiteSpace(Username)) {
    //      return null;
    //    }
    //    return new PSCredential(Username, SecurePassword);
    //  }
    //}
    #endregion Public properties

    #region Constructor(s)
    public TCredential() : base() {
      XmlSecure = false;
      EncryptionKey = "";
    }
    public TCredential(string username, string password, bool xmlSecure = true) : base() {
      Username = username;
      SecurePassword = password.ConvertToSecureString();
      XmlSecure = xmlSecure;
      EncryptionKey = "";
    }
    public TCredential(string username, SecureString password, bool xmlSecure = true) : base() {
      Username = username;
      SecurePassword = password;
      XmlSecure = xmlSecure;
      EncryptionKey = "";
    }
    public TCredential(ICredential credential) : base() {
      if ( credential == null ) {
        return;
      }
      Username = credential.Username ?? "";
      SecurePassword = credential.SecurePassword;
      XmlSecure = credential.XmlSecure;
      EncryptionKey = "";
    }
    public TCredential(ICredential credential, IParent parent) : base() {
      if ( credential == null ) {
        return;
      }
      Username = credential.Username ?? "";
      SecurePassword = credential.SecurePassword;
      XmlSecure = credential.XmlSecure;
      EncryptionKey = "";
      Parent = parent;
    }
    public TCredential(XElement credential, IParent parent) : base() {
      #region Validate parameters
      if ( credential == null || !credential.HasAttributes ) {
        Log.Write("Unable to create TCredential from XML : XElement is empty or invalid", ErrorLevel.Error);
        return;
      }
      #endregion Validate parameters

      Parent = parent;
      Username = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_USERNAME, "");
      XmlSecure = credential.SafeReadAttribute<bool>(XML_ATTRIBUTE_XMLSECURE, false);

      if ( parent == null || !(parent is IName) ) {
        EncryptionKey = "";
      } else {
        EncryptionKey = MakeKey(((IName)parent).Name);
      }

      string PasswordFromXElement = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_PASSWORD, "");
      if ( XmlSecure ) {
        try {
          SecurePassword = PasswordFromXElement.DecryptFromBase64(EncryptionKey).ConvertToSecureString();
        } catch ( Exception ex ) {
          Log.Write($"Problem with decryption of the password {PasswordFromXElement} : {ex.Message}", ErrorLevel.Error);
          SecurePassword = "".ConvertToSecureString();
        }
      } else {
        SecurePassword = PasswordFromXElement.ConvertToSecureString();
      }

    }
    public TCredential(XElement credential, string encryptionKey = "") : base() {
      #region Validate parameters
      if ( credential == null || !credential.HasAttributes ) {
        Log.Write("Unable to create TCredential from XML : XElement is empty or invalid", ErrorLevel.Error);
        return;
      }
      #endregion Validate parameters
      Username = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_USERNAME, "");
      if ( encryptionKey != "" ) {
        EncryptionKey = encryptionKey;
      }
      XmlSecure = credential.SafeReadAttribute<bool>(XML_ATTRIBUTE_XMLSECURE, false);

      string PasswordFromXElement = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_PASSWORD, "");
      if ( XmlSecure ) {
        try {
          SecurePassword = PasswordFromXElement.DecryptFromBase64(EncryptionKey).ConvertToSecureString();
        } catch ( Exception ex ) {
          Log.Write($"Problem with decryption of the password {PasswordFromXElement} : {ex.Message}", ErrorLevel.Error);
          SecurePassword = "".ConvertToSecureString();
        }
      } else {
        SecurePassword = PasswordFromXElement.ConvertToSecureString();
      }

    }

    protected override void _Initialize() { }

    public override void Dispose() {
      if ( SecurePassword != null ) {
        SecurePassword.Dispose();
      }
    }
    #endregion Constructor(s)

    #region Converters
    public XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);
      if ( Inherited ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_INHERITED, Inherited);
      } else {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_USERNAME, Username);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_XMLSECURE, XmlSecure);
        if ( SecurePassword != null ) {
          if ( XmlSecure ) {
            RetVal.SetAttributeValue(XML_ATTRIBUTE_PASSWORD, SecurePassword.ConvertToUnsecureString().EncryptToBase64(EncryptionKey));
          } else {
            RetVal.SetAttributeValue(XML_ATTRIBUTE_PASSWORD, SecurePassword.ConvertToUnsecureString());
          }
        }
      }
      return RetVal;
    }

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      if ( Inherited ) {
        RetVal.Append("(Inherited)");
      } else {
        RetVal.Append(Username);
        if ( !XmlSecure ) {
          RetVal.AppendFormat(", {0}", SecurePassword.ConvertToUnsecureString());
        } else {
          RetVal.AppendFormat(", {0}", SecurePassword.ConvertToUnsecureString().EncryptToBase64(EncryptionKey));
        }
      }
      return RetVal.ToString();
    }
    #endregion Converters

    #region Public static Create
    public static TCredential Create() {
      return new TCredential();
    }
    public static TCredential Create(string username, string password) {
      return new TCredential(username, password);
    }
    public static TCredential Create(string username, SecureString password) {
      return new TCredential(username, password);
    }
    public static TCredential Create(ICredential credential) {
      return new TCredential(credential);
    }
    public static TCredential Create(XElement credential, IParent parent) {
      return new TCredential(credential, parent);
    }
    #endregion Public static Create

    public void SetSecure(bool value) {
      XmlSecure = value;
    }

    private string MakeKey(string source) {
      if ( source == null ) {
        return "";
      }
      return new string(source.Reverse().ToArray());
    }
  }
}
