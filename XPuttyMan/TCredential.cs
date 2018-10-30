using EasyPutty.Base;
using BLTools;
using BLTools.Encryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyPutty {
  public class TCredential : TEasyPuttyBase, IToXml, IDisposable, ICredential {

    /// <summary>
    /// Set this to true to view additional debug message
    /// </summary>
    public static bool IsDebug = false;

    #region XML contants
    public static XName XML_THIS_ELEMENT {
      get {
        return GetXName("Credential");
      }
    }
    public const string XML_ATTRIBUTE_USERNAME = "Username";
    public const string XML_ATTRIBUTE_XMLSECURE = "Secure";
    public const string XML_ATTRIBUTE_PASSWORD = "Password";
    public const string XML_ATTRIBUTE_INHERITED = "Inherited";
    #endregion XML contants

    #region Public properties
    public string Username { get; protected set; }

    public string Domain {
      get {
        if (string.IsNullOrWhiteSpace(Username)) {
          return "";
        }
        if (!Username.Contains(@"\") && !Username.Contains("@")) {
          return Username;
        }
        if (Username.Contains(@"\")) {
          return Username.Left(Username.IndexOf(@"\"));
        }
        return Username.Left(Username.IndexOf("@"));
      }
    }
    public string UsernameWithoutDomain {
      get {
        if (string.IsNullOrWhiteSpace(Username)) {
          return "";
        }
        if (!Username.Contains(@"\") && !Username.Contains("@")) {
          return "";
        }
        if (Username.Contains(@"\")) {
          return Username.Substring(Username.IndexOf(@"\") + 1);
        }
        return Username.Substring(Username.IndexOf("@") + 1);
      }
    }

    private bool _XmlSecure;
    public bool XmlSecure {
      get {
        if (Inherited) {
          return true;
        }
        return _XmlSecure;
      }
      set {
        _XmlSecure = value;
      }
    }

    public SecureString SecurePassword { get; protected set; }

    private string _EncryptionKey = "";

    public string EncryptionKey {
      private get {
        if (!string.IsNullOrWhiteSpace(_EncryptionKey)) {
          return _EncryptionKey;
        }
        return MakeKey(ParentName);
      }
      set {
        if (!string.IsNullOrWhiteSpace(value)) {
          _EncryptionKey = MakeKey(value);
        } else {
          _EncryptionKey = MakeKey(ParentName);
        }
      }
    }

    public bool HasValue {
      get {
        return !string.IsNullOrWhiteSpace(Username);
      }
    }
    public bool Inherited {
      get {
        
        if (HasValue) {
          return false;
        }
        if (Parent == null) {
          return false;
        }

        ICredentialContainer ParentContainer = Parent.Parent as ICredentialContainer;

        if (ParentContainer == null) {
          return false;
        }

        if (ParentContainer.Credential.HasValue) {
          NotifyExecutionProgress($"Credential is coming from Parent => {ParentContainer.GetType().Name}:{ParentName}");
          return true;
        }

        return ParentContainer.Credential.Inherited;
      }
    }

    public string ParentName {
      get {
        if (Parent is IName) {
          IName ParentWithName = Parent as IName;
          return ParentWithName.Name;
        }
        return "";
      }
    }

    public PSCredential PsCredential {
      get {
        if (string.IsNullOrWhiteSpace(Username)) {
          return null;
        }
        return new PSCredential(Username, SecurePassword);
      }
    }
    #endregion Public properties

    #region Constructor(s)
    public TCredential() {
      XmlSecure = false;
      EncryptionKey = "";
    }
    public TCredential(string username, string password, bool xmlSecure = true) : this() {
      Username = username;
      SecurePassword = password.ConvertToSecureString();
      XmlSecure = xmlSecure;
    }
    public TCredential(string username, SecureString password, bool xmlSecure = true) : this() {
      Username = username;
      SecurePassword = password;
      XmlSecure = xmlSecure;
    }
    public TCredential(ICredential credential) : this() {
      if (credential == null) {
        return;
      }
      Username = credential.Username ?? "";
      SecurePassword = credential.SecurePassword;
      XmlSecure = credential.XmlSecure;
    }
    public TCredential(XElement credential, IParent parent) : this() {
      #region Validate parameters
      if (credential == null || !credential.HasAttributes) {
        Trace.WriteLineIf(IsDebug, "Unable to create TCredential from XML : XElement is empty or invalid");
        return;
      }
      #endregion Validate parameters

      Parent = parent;
      NotifyExecutionProgress($"Creating Credential for {ParentName}");
      Username = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_USERNAME, "");
      XmlSecure = credential.SafeReadAttribute<bool>(XML_ATTRIBUTE_XMLSECURE, false);

      if (parent == null || !(parent is IName)) {
        EncryptionKey = "";
      } else {
        EncryptionKey = MakeKey(((IName)parent).Name);
      }

      string PasswordFromXElement = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_PASSWORD, "");
      if (XmlSecure) {
        try {
          SecurePassword = PasswordFromXElement.DecryptFromBase64(EncryptionKey).ConvertToSecureString();
        } catch (Exception ex) {
          NotifyExecutionError($"Problem with decryption of the password {PasswordFromXElement} : {ex.Message}", ErrorLevel.Error);
          SecurePassword = "".ConvertToSecureString();
        }
      } else {
        SecurePassword = PasswordFromXElement.ConvertToSecureString();
      }

    }
    public TCredential(XElement credential, string encryptionKey = "") : this() {
      #region Validate parameters
      if (credential == null || !credential.HasAttributes) {
        Trace.WriteLineIf(IsDebug, "Unable to create TCredential from XML : XElement is empty or invalid");
        return;
      }
      #endregion Validate parameters
      Username = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_USERNAME, "");
      if (encryptionKey != "") {
        EncryptionKey = encryptionKey;
      }
      XmlSecure = credential.SafeReadAttribute<bool>(XML_ATTRIBUTE_XMLSECURE, false);

      string PasswordFromXElement = credential.SafeReadAttribute<string>(XML_ATTRIBUTE_PASSWORD, "");
      if (XmlSecure) {
        try {
          //NotifyExecutionProgress("Decrypting Credential");
          //NotifyExecutionProgress($"EncryptionKey is [{EncryptionKey.ToString()}]");
          SecurePassword = PasswordFromXElement.DecryptFromBase64(EncryptionKey).ConvertToSecureString();
          //NotifyExecutionProgress($"SecurePassword is {SecurePassword.ConvertToUnsecureString()}");
        } catch (Exception ex) {
          Trace.WriteLine(string.Format("Problem with decryption of the password {0} : {1}", PasswordFromXElement, ex.Message), Severity.Error);
          SecurePassword = "".ConvertToSecureString();
        }
      } else {
        SecurePassword = PasswordFromXElement.ConvertToSecureString();
      }

    }

    public new void Dispose() {
      if (SecurePassword != null) {
        SecurePassword.Dispose();
      }
    }
    #endregion Constructor(s)

    #region Converters
    public override XElement ToXml() {
      NotifyExecutionProgress("Local Credential ToXml");
      XElement RetVal = base.ToXml(XML_THIS_ELEMENT);
      if (Inherited) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_INHERITED, Inherited);
      } else {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_USERNAME, Username);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_XMLSECURE, XmlSecure);
        if (SecurePassword != null) {
          if (XmlSecure) {
            //NotifyExecutionProgress($"SecurePassword is {SecurePassword.ConvertToUnsecureString()}");
            //NotifyExecutionProgress($"EncryptionKey is [{EncryptionKey.ToString()}]");
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
      if (Inherited) {
        RetVal.Append("(Inherited)");
      } else {
        RetVal.Append(Username);
        if (!XmlSecure) {
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
      if (source == null) {
        return "";
      }
      return new string(source.Reverse().ToArray());
    }
  }
}
