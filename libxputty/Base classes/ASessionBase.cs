using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using BLTools;
using BLTools.Diagnostic.Logging;

namespace libxputty {

  public abstract class ASessionBase : ALoggable, IName, IDisposable, IParent, ICredentialContainer, IToXml {

    #region XML constants
    public const string XML_ATTRIBUTE_NAME = nameof(Name);
    public const string XML_ATTRIBUTE_DESCRIPTION = nameof(Description);
    public const string XML_ATTRIBUTE_COMMENT = nameof(Comment);

    public static string DefaultXmlNamespace
    {
      get
      {
        return "http://easyputty.sharenet.be";
      }
    }

    public static string XmlNamespace
    {
      get
      {
        if (_XmlNamespace == null) {
          return DefaultXmlNamespace;
        } else {
          return _XmlNamespace;
        }
      }
      set
      {
        _XmlNamespace = value;
      }
    }
    protected static string _XmlNamespace;

    public static XName GetXName(string name = "") {
      if (string.IsNullOrEmpty(name)) {
        return XName.Get(XmlNamespace);
      } else {
        return XName.Get(name, XmlNamespace);
      }
    }

    public static XName GetXName(string name, string xmlNamespace) {
      if (string.IsNullOrEmpty(name)) {
        return XName.Get(xmlNamespace);
      } else {
        return XName.Get(name, xmlNamespace);
      }
    }
    #endregion XML constants

    #region --- IName --------------------------------------------
    /// <summary>
    /// Name of the item
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Description of the item
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// A comment
    /// </summary>
    public string Comment { get; set; } = "";
    #endregion --- IName --------------------------------------------

    /// <summary>
    /// The location where to save or load information
    /// </summary>
    public string StorageLocation { get; set; }

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    protected ASessionBase() {
      _Initialize();
    }

    protected ASessionBase(string name) {
      Name = name;
      _Initialize();
    }

    protected ASessionBase(ISession sessionBase) {
      if (sessionBase is null) {
        return;
      }
      Name = sessionBase.Name;
      Description = sessionBase.Description;
      Comment = sessionBase.Comment;
      StorageLocation = sessionBase.StorageLocation;
      Parent = sessionBase.Parent;
      _Initialize();
    }

    private bool _IsInitialized = false;
    protected virtual void _Initialize() {
      if (_IsInitialized) {
        return;
      }

      _IsInitialized = true;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region ICredentialContainer
    /// <summary>
    /// The credential to use when executing an action
    /// </summary>
    public virtual ICredential Credential
    {
      get
      {
        // If local credential, use it
        if (_LocalCredential is not null) {
          return _LocalCredential;
        }

        // If a parent is ICredentialContainer, use it
        if (Parent is not null && Parent is ICredentialContainer ParentWithCredential) {
          return ParentWithCredential.Credential;
        }

        // No Credential available
        return null;
      }
    }
    protected ICredential _LocalCredential;

    public void SetLocalCredential(ICredential credential) {
      if (credential is null) {
        return;
      }
      if (credential.HasValue) {
        _LocalCredential = new TCredential(credential, this);
      }
    }

    public virtual void SetSecure(bool value, bool recurse = true) {
      if (_LocalCredential is not null) {
        _LocalCredential.SetSecure(value);
      }

      if (!recurse) {
        return;
      }

      PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
      if (ItemsProperty is null) {
        return;
      }

      if (ItemsProperty.GetValue(this) is not IEnumerable<object> LocalItems) {
        return;
      }

      foreach (ICredentialContainer CredentialContainerItem in LocalItems.OfType<ICredentialContainer>()) {
        CredentialContainerItem.SetSecure(value, recurse);
      }

    }

    public bool IsCredentialInherited => _LocalCredential is null && Parent is not null && Parent is ICredentialContainer;

    public virtual bool HasUnsecuredPassword
    {
      get
      {
        if (_LocalCredential is not null && !_LocalCredential.XmlSecure) {
          return true;
        }

        // If tested item has sub-items, test them too
        PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
        if (ItemsProperty is not null) {
          if (ItemsProperty.GetValue(this) is IEnumerable<object> LocalItems) {
            return LocalItems.OfType<ICredentialContainer>().Any(x => x.HasUnsecuredPassword);
          }
        }

        return false;
      }
    }
    #endregion ICredentialContainer

    #region Converters
    public virtual XElement ToXml(string name) {
      return ToXml(GetXName(name));
    }
    public virtual XElement ToXml(XName name) {
      #region Validate parameters
      if (name == null) {
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

      if (!IsCredentialInherited) {
        RetVal.Add(_LocalCredential.ToXml());
      }

      return RetVal;
    }

    #region --- IToXml --------------------------------------------
    public virtual XElement ToXml() {
      throw new NotImplementedException();
    }

    public virtual void FromXml(XElement source) {
      throw new NotImplementedException();
    }
    #endregion --- IToXml --------------------------------------------

    #endregion Converters

    #region IParent
    public IParent Parent { get; set; }
    public T GetParent<T>() {
      if (Parent is null) {
        return default(T);
      }
      if (Parent.GetType().Name == typeof(T).Name) {
        return (T)Convert.ChangeType(Parent, typeof(T));
      }
      return Parent.GetParent<T>();
    }
    #endregion IParent

    #region IDisposable Support
    private bool _DisposedValue = false; // To detect redundant calls

    protected virtual void _Dispose(bool disposing) {
      if (!_DisposedValue) {
        if (disposing) {
          // TODO: dispose managed state (managed objects).
        }

        Parent = null;
        if (Credential != null) {
          Credential.Dispose();
        }
        _DisposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public virtual void Dispose() {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      _Dispose(true);

    }
    #endregion IDisposable Support

  }
}
