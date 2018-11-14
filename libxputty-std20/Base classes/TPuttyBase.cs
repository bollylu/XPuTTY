using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using BLTools;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public abstract class TPuttyBase : IName, IDisposable, IParent, ICredentialContainer {

    #region XML constants
    public const string XML_ATTRIBUTE_NAME = "Name";
    public const string XML_ATTRIBUTE_DESCRIPTION = "Description";
    public const string XML_ATTRIBUTE_COMMENT = "Comment";

    public static string DefaultXmlNamespace {
      get {
        return "http://easyputty.sharenet.be";
      }
    }

    public static string XmlNamespace {
      get {
        if ( _XmlNamespace == null ) {
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

    public static XName GetXName(string name = "") {
      if ( name == "" ) {
        return XName.Get(XmlNamespace);
      } else {
        return XName.Get(name, XmlNamespace);
      }
    }

    public static XName GetXName(string name, string xmlNamespace) {
      if ( name == "" ) {
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
    /// The location where to save or load data
    /// </summary>
    public string StorageLocation { get; set; }

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttyBase() {
      _Initialize();
    }

    public TPuttyBase(string name) {
      Name = name;
      _Initialize();
    }

    public TPuttyBase(TPuttyBase puttyBase) {
      if ( puttyBase == null ) {
        return;
      }
      Name = puttyBase.Name;
      Description = puttyBase.Description;
      Comment = puttyBase.Comment;
      StorageLocation = puttyBase.StorageLocation;
      Parent = puttyBase.Parent;
      LocalCredential = puttyBase.LocalCredential;
    }

    protected abstract void _Initialize();
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region ICredentialContainer
    /// <summary>
    /// The credential to use when executing an action
    /// </summary>
    public virtual ICredential Credential {
      get {
        // If local credential, use it
        if ( LocalCredential != null ) {
          return LocalCredential;
        }

        // If a parent is ICredentialContainer, use it
        if ( Parent != null && Parent is ICredentialContainer ParentWithCredential ) {
          return ParentWithCredential.Credential;
        }

        // No Credential available
        return null;
      }
    }
    protected ICredential LocalCredential;

    public void SetLocalCredential(ICredential credential) {
      if ( credential == null ) {
        return;
      }
      if ( credential.HasValue ) {
        LocalCredential = new TCredential(credential, this);
      }
    }

    public virtual void SetSecure(bool value, bool recurse = true) {
      if ( LocalCredential != null ) {
        LocalCredential.SetSecure(value);
      }

      if ( !recurse ) {
        return;
      }

      PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
      if ( ItemsProperty == null ) {
        return;
      }

      if ( !(ItemsProperty.GetValue(this) is IEnumerable<object> LocalItems) ) {
        return;
      }

      foreach ( ICredentialContainer CredentialContainerItem in LocalItems.OfType<ICredentialContainer>() ) {
        CredentialContainerItem.SetSecure(value, recurse);
      }

    }

    public bool IsCredentialInherited => Parent != null && Parent is ICredentialContainer && LocalCredential == null;

    public virtual bool HasUnsecuredPassword {
      get {
        if ( LocalCredential != null && !LocalCredential.XmlSecure ) {
          return true;
        }

        // If tested item has sub-items, test them too
        PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
        if ( ItemsProperty != null ) {
          if ( ItemsProperty.GetValue(this) is IEnumerable<object> LocalItems ) {
            return LocalItems.Where(x => x is ICredentialContainer).Cast<ICredentialContainer>().Any(x => x.HasUnsecuredPassword);
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
      if ( name == null ) {
        return new XElement("");
      }
      #endregion Validate parameters

      XElement RetVal = new XElement(name);

      if ( !string.IsNullOrWhiteSpace(Name) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      }

      if ( !string.IsNullOrWhiteSpace(Description) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_DESCRIPTION, Description);
      }

      if ( !string.IsNullOrWhiteSpace(Comment) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_COMMENT, Comment);
      }

      if ( !IsCredentialInherited && LocalCredential != null ) {
        RetVal.Add(LocalCredential.ToXml());
      }

      return RetVal;
    }
    #endregion Converters

    #region IParent
    public IParent Parent { get; set; }
    public T GetParent<T>() {
      if ( Parent == null ) {
        return default(T);
      }
      if ( Parent.GetType().Name == typeof(T).Name ) {
        return (T)Convert.ChangeType(Parent, typeof(T));
      }
      return Parent.GetParent<T>();
    }
    #endregion IParent

    

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if ( !disposedValue ) {
        if ( disposing ) {
          // TODO: dispose managed state (managed objects).
        }

        Parent = null;
        if ( Credential != null ) {
          Credential.Dispose();
        }
        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public virtual void Dispose() {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);

    }
    #endregion IDisposable Support

  }
}
