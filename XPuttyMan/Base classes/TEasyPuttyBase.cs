using BLTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using BLTools.MVVM;

namespace EasyPutty.Base {
  public abstract class TEasyPuttyBase : MVVMBase, IName, IParent, ICredentialContainer, IDisposable, IToXml {

    #region XML constants
    public const string XML_ATTRIBUTE_NAME = "Name";
    public const string XML_ATTRIBUTE_DESCRIPTION = "Description";
    public const string XML_ATTRIBUTE_COMMENT = "Comment";

    public static string DefaultXmlNamespace {
      get {
        return "http://srvtfsailg.ai.usinor.com/Support";
      }
    }

    public static string XmlNamespace {
      get {
        if (_XmlNamespace == null) {
          return DefaultXmlNamespace;
        } else {
          return _XmlNamespace;
        }
      }
      set {
        _XmlNamespace = value;
      }
    }
    protected static string _XmlNamespace;

    public static XName GetXName(string name="") {
      if (name == "") {
        return XName.Get(XmlNamespace);
      } else {
        return XName.Get(name, XmlNamespace);
      }
    }

    public static XName GetXName(string name, string xmlNamespace) {
      if (name == "") {
        return XName.Get(xmlNamespace);
      } else {
        return XName.Get(name, xmlNamespace);
      }
    }
    #endregion XML constants

    #region Public properties
    /// <summary>
    /// Name of the item
    /// </summary>
    public virtual string Name {
      get {
        return _Name;
      }
      protected set {
        if (value != _Name) {
          _Name = value;
          NotifyPropertyChanged(nameof(Name));
        }
      }
    }
    protected string _Name;

    /// <summary>
    /// Description of the item
    /// </summary>
    public virtual string Description {
      get {
        return _Description;
      }
      set {
        if (value != _Description) {
          _Description = value;
        }
        NotifyPropertyChanged(nameof(Description));
      }
    }
    protected string _Description;

    /// <summary>
    /// A comment
    /// </summary>
    public virtual string Comment {
      get {
        return _Comment;
      }
      set {
        if (value != _Comment) {
          _Comment = value;
          NotifyPropertyChanged(nameof(Comment));
        }
      }
    }
    protected string _Comment;

    /// <summary>
    /// The location where to save or load data
    /// </summary>
    public string StorageLocation {
      get {
        return _StorageLocation;
      }
      set {
        if (value != _StorageLocation) {
          _StorageLocation = value;
          NotifyPropertyChanged(nameof(StorageLocation));
        }
      }
    }
    protected string _StorageLocation;
    #endregion Public properties

    #region Constructor(s)
    public TEasyPuttyBase() {
      Initialize();
    }

    public TEasyPuttyBase(string name) : this() {
      Name = name;
    }

    public TEasyPuttyBase(XElement element) : this() {
      if (element == null) {
        return;
      }

      Name = element.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");
      Description = element.SafeReadAttribute<string>(XML_ATTRIBUTE_DESCRIPTION, "");
      Comment = element.SafeReadAttribute<string>(XML_ATTRIBUTE_COMMENT, "");

      if (element.Elements(TCredential.XML_THIS_ELEMENT).Count() > 0) {
        SetLocalCredential(new TCredential(element.SafeReadElement(TCredential.XML_THIS_ELEMENT), Name));
      }

    }

    public TEasyPuttyBase(TEasyPuttyBase easyPutty) {
      if (easyPutty == null) {
        return;
      }
      Name = easyPutty.Name;
      Description = easyPutty.Description;
      Comment = easyPutty.Comment;
      StorageLocation = easyPutty.StorageLocation;
      Parent = easyPutty.Parent;
      LocalCredential = easyPutty.LocalCredential;
    }

    protected virtual void Initialize() {
      if (Name == null) {
        Name = "";
      }
      if (Description == null) {
        Description = "";
      }
      if (Comment == null) {
        Comment = "";
      }
    }
    #endregion Constructor(s)



    #region Converters
    public abstract XElement ToXml();
    public virtual XElement ToXml(string name) {
      return ToXml(GetXName(name));
    }
    public virtual XElement ToXml(XName name) {
      #region Validate parameters
      if (name == null) {
        NotifyExecutionError($"Unable to transform {this.GetType().Name} to Xml : XName is null");
        return new XElement("");
      }
      #endregion Validate parameters
      XElement RetVal = new XElement(name);

      if (!string.IsNullOrWhiteSpace(Name)) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      }

      if (!string.IsNullOrWhiteSpace(Description)) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_DESCRIPTION, Description);
      }

      if (!string.IsNullOrWhiteSpace(Comment)) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_COMMENT, Comment);
      }

      if (!IsCredentialInherited && LocalCredential != null) {
        RetVal.Add(LocalCredential.ToXml());
      }

      return RetVal;
    }
    #endregion Converters

    #region IParent
    public IParent Parent { get; set; }
    public T GetParent<T>() {
      if (Parent == null) {
        return default(T);
      }
      if (Parent.GetType().Name == typeof(T).Name) {
        return (T)Convert.ChangeType(Parent, typeof(T));
      }
      return Parent.GetParent<T>();
    }
    #endregion IParent

    #region ICredentialContainer
    /// <summary>
    /// The credential to use when executing an action against the object (computer, cluster, ...)
    /// </summary>
    public virtual ICredential Credential {
      get {
        // If local credential, use it
        if (LocalCredential != null) {
          return LocalCredential;
        }

        // If a parent is ICredentialContainer, use it
        if (Parent != null && Parent is ICredentialContainer) {
          ICredentialContainer ParentWithCredential = Parent as ICredentialContainer;
          return ParentWithCredential.Credential;
        }

        // No Credential available
        return null;
      }
    }
    protected ICredential LocalCredential;

    public void SetLocalCredential(ICredential credential) {
      if (credential == null) {
        return;
      }
      if (credential.HasValue) {
        NotifyExecutionProgress($"Adding credential to {Name}");
        LocalCredential = credential;
        LocalCredential.Parent = this;
      } else {
        NotifyExecutionProgress($"Credential for {Name} will be inherited");
      }
    }
    public virtual void SetSecure(bool value, bool recurse = true) {
      if (LocalCredential != null) {
        LocalCredential.SetSecure(value);
      }
      if (recurse) {
        PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
        if (ItemsProperty != null) {
          var LocalItems = ItemsProperty.GetValue(this) as IEnumerable<object>;
          if (LocalItems != null) {
            foreach (ICredentialContainer CredentialContainerItem in LocalItems.Where(x => x is ICredentialContainer)) {
              CredentialContainerItem.SetSecure(value, recurse);
            }
          }
        }
      }

    }
    public bool IsCredentialInherited {
      get {
        return Parent != null && Parent is ICredentialContainer && LocalCredential == null;
      }
    }

    public virtual bool HasUnsecuredPassword {
      get {
        NotifyExecutionProgress($"Test {this.GetType().Name}:{Name} for HasUnsecuredPassword...");
        if (LocalCredential != null && !LocalCredential.XmlSecure) {
          return true;
        }

        // If tested item has sub-items, test them too
        PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
        if (ItemsProperty != null) {
          var LocalItems = ItemsProperty.GetValue(this) as IEnumerable<object>;
          if (LocalItems != null) {
            return LocalItems.Where(x => x is ICredentialContainer).Cast<ICredentialContainer>().Any(x => x.HasUnsecuredPassword);
          }
        }

        return false;
      }
    }
    #endregion ICredentialContainer

    #region Xml IO
    public virtual bool SaveXml(string storageLocation = "") {
      #region Validate parameters
      if (!string.IsNullOrWhiteSpace(storageLocation)) {
        StorageLocation = storageLocation;
      }
      #endregion Validate parameters
      ClearExecutionStatus();
      XDocument XmlFile = new XDocument();
      XmlFile.Declaration = new XDeclaration("1.0", Encoding.UTF8.EncodingName, "true");
      XmlFile.Add(this.ToXml());
      try {
        NotifyExecutionProgress($"Saving data {this.GetType().Name} to file {StorageLocation} ...");
        XmlFile.Save(StorageLocation);
        ClearExecutionProgress();
        NotifyExecutionStatus("SaveXml successful");
        return true;
      } catch (Exception ex) {
        ClearExecutionProgress();
        NotifyExecutionError($"Unable to save information to file {StorageLocation} : {ex.Message}", ErrorLevel.Error);
        NotifyExecutionStatus("SaveXml failed");
        return false;
      }
    }

    public virtual XElement LoadXml(string storageLocation = "") {
      #region Validate parameters
      if (!string.IsNullOrWhiteSpace(storageLocation)) {
        StorageLocation = storageLocation;
      }
      if (!File.Exists(StorageLocation)) {
        NotifyExecutionError($"Unable to read information from file {StorageLocation} : incorrect or missing filename", ErrorLevel.Error);
        return null;
      }
      #endregion Validate parameters
      XDocument XmlFile;
      try {
        ClearExecutionStatus();
        NotifyExecutionProgress("Reading file content...");
        XmlFile = XDocument.Load(StorageLocation);

        NotifyExecutionProgress("Parsing content...");
        XElement Root = XmlFile.Root;
        if (Root == null) {
          NotifyExecutionError("unable to read config file content");
          return null;
        }

        NotifyExecutionStatus("LoadXml Sucessfull");
        return Root;
      } catch (Exception ex) {
        NotifyExecutionError($"Unable to read information from file {StorageLocation} : {ex.Message}", ErrorLevel.Error);
        NotifyExecutionStatus("LoadXml failed");
        return null;
      }
    }
    #endregion Xml IO

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          // TODO: dispose managed state (managed objects).
        }

        Parent = null;
        if (Credential != null) {
          Credential.Dispose();
        }
        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);

    }
    #endregion IDisposable Support

  }
}
