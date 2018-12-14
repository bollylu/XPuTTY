using System;

using BLTools;
using BLTools.MVVM;
using EasyPutty.Interfaces;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public abstract class TVMEasyPuttyBase : MVVMBase, IParent, IDisposable, IDataDirty {

    protected object _Data { get; set; }
    protected object _Lock = new object();
    
    #region Public properties
    /// <summary>
    /// Name of the item
    /// </summary>
    public virtual string Name {
      get {
        if ( _Data != null && _Data is IName DataWithName ) {
          return DataWithName.Name;
        }
        return "";
      }
    }

    /// <summary>
    /// Description of the item
    /// </summary>
    public virtual string Description {
      get {
        if ( _Data != null && _Data is IName DataWithName ) {
          return DataWithName.Description;
        }
        return "";
      }
      set {
        if ( _Data != null && _Data is IName DataWithName && DataWithName.Description != value ) {
          DataWithName.Description = value;
          NotifyPropertyChanged(nameof(Description));
        }
      }
    }

    /// <summary>
    /// A comment
    /// </summary>
    public virtual string Comment {
      get {
        if ( _Data != null && _Data is IName DataWithName ) {
          return DataWithName.Comment;
        }
        return "";
      }
      set {
        if ( _Data != null && _Data is IName DataWithName && DataWithName.Comment != value ) {
          DataWithName.Comment = value;
          NotifyPropertyChanged(nameof(Comment));
        }
      }
    }

    /// <summary>
    /// The location where to save or load data
    /// </summary>
    public virtual string StorageLocation {
      get {
        if ( _Data != null && _Data is IStorage DataWithName ) {
          return DataWithName.StorageLocation;
        }
        return "";
      }
      set {
        if ( _Data != null && _Data is IStorage DataWithName && DataWithName.StorageLocation != value ) {
          DataWithName.StorageLocation = value;
          NotifyPropertyChanged(nameof(StorageLocation));
        }
      }
    }
    #endregion Public properties

    #region Constructor(s)
    public TVMEasyPuttyBase() : base() {
      ResetDataDirty();
      _InitializeCommands();
      _Initialize();
    }

    public TVMEasyPuttyBase(object data) : base() {
      _Data = data;
      ResetDataDirty();
      _InitializeCommands();
      _Initialize();
    }

    public TVMEasyPuttyBase(TVMEasyPuttyBase vmEasyPutty) : base() {
      if ( vmEasyPutty == null ) {
        return;
      }
      _Data = vmEasyPutty._Data;
      Parent = vmEasyPutty.Parent;
      ResetDataDirty();
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
      if ( Parent == null ) {
        return default(T);
      }
      if ( Parent.GetType().Name == typeof(T).Name ) {
        return (T)Convert.ChangeType(Parent, typeof(T));
      }
      return Parent.GetParent<T>();
    }
    #endregion IParent

    #region --- IDataDirty --------------------------------------------
    public bool IsDataDirty {
      get {
        if ( _Data is IDataDirty DataWithDirtyState ) {
          return (_DataIsDirty || DataWithDirtyState.IsDataDirty);
        }
        return _DataIsDirty;
      }
    }
    private bool _DataIsDirty;

    public void ResetDataDirty(bool recurse = false) {
      _DataIsDirty = false;
      if ( recurse && _Data is IDataDirty DataWithDirtyState ) {
        DataWithDirtyState.ResetDataDirty(recurse);
      }
      NotifyPropertyChanged(nameof(IsDataDirty));
    }

    public void SetDataDirty() {
      _DataIsDirty = true;
      NotifyPropertyChanged(nameof(IsDataDirty));
    }
    #endregion --- IDataDirty -----------------------------------------

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if ( !disposedValue ) {
        if ( disposing ) {
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
