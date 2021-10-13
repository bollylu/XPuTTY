using System;
using System.Xml.Linq;

using BLTools;
using BLTools.MVVM;

using libxputty.Interfaces;

namespace EasyPutty.ViewModels {
  public abstract class AVMEasyPuttyBase : AMVVM, IParent, IDisposable {

    protected object _Data { get; set; }
    protected readonly object _Lock = new();

    #region Public properties
    public virtual ICredential Credential { get; protected set; }

    /// <summary>
    /// Name of the item
    /// </summary>
    public virtual string Name
    {
      get
      {
        if (_Data is not null && _Data is IName DataWithName) {
          return DataWithName.Name;
        }
        return "";
      }
    }

    /// <summary>
    /// Description of the item
    /// </summary>
    public virtual string Description
    {
      get
      {
        if (_Data != null && _Data is IName DataWithName) {
          return DataWithName.Description;
        }
        return "";
      }
      set
      {
        if (_Data != null && _Data is IName DataWithName && DataWithName.Description != value) {
          DataWithName.Description = value;
          NotifyPropertyChanged(nameof(Description));
        }
      }
    }

    /// <summary>
    /// A comment
    /// </summary>
    public virtual string Comment
    {
      get
      {
        if (_Data != null && _Data is IName DataWithName) {
          return DataWithName.Comment;
        }
        return "";
      }
      set
      {
        if (_Data != null && _Data is IName DataWithName && DataWithName.Comment != value) {
          DataWithName.Comment = value;
          NotifyPropertyChanged(nameof(Comment));
        }
      }
    }

    /// <summary>
    /// The location where to save or load data
    /// </summary>
    public virtual string StorageLocation
    {
      get
      {
        if (_Data != null && _Data is IStorage DataWithName) {
          return DataWithName.StorageLocation;
        }
        return "";
      }
      set
      {
        if (_Data != null && _Data is IStorage DataWithName && DataWithName.StorageLocation != value) {
          DataWithName.StorageLocation = value;
          NotifyPropertyChanged(nameof(StorageLocation));
        }
      }
    }
    #endregion Public properties

    #region Constructor(s)
    public AVMEasyPuttyBase() : base() {
      _InitializeCommands();
      _Initialize();
    }

    public AVMEasyPuttyBase(object data) : base() {
      _Data = data;
      _InitializeCommands();
      _Initialize();
    }

    public AVMEasyPuttyBase(AVMEasyPuttyBase vmEasyPutty) : base() {
      if (vmEasyPutty is null) {
        return;
      }
      _Data = vmEasyPutty._Data;
      Parent = vmEasyPutty.Parent;
      _Initialize();
    }

    protected abstract void _InitializeCommands();
    protected abstract void _Initialize();
    #endregion Constructor(s)

    #region Converters
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
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          _Data = null;
        }

        Parent = null;
        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public abstract void Dispose();


    //  {
    //  // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //  Dispose(true);

    //}
    #endregion IDisposable Support

  }
}
