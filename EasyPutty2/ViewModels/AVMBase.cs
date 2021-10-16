using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using BLTools;
using EasyPutty;
using EasyPutty.Models;

using libxputty;

namespace EasyPutty.ViewModels {
  public abstract class AVMBase : AVMEasyPuttyBase, IEquatable<AVMBase> {

    public override ICredential Credential
    {
      get
      {
        if (_Data is ICredentialContainer) {
          ICredentialContainer DataWithCredential = _Data as ICredentialContainer;
          return DataWithCredential.Credential;
        }
        return null;
      }
    }

    public override string Description {
      get {
        IName DataWithDescription = _Data as IName;
        if ( DataWithDescription != null ) {
          return DataWithDescription.Description;
        }
        return null;
      }

      set {
        IName DataWithDescription = _Data as IName;
        if ( DataWithDescription != null ) {
          DataWithDescription.Description = value;
        }
      }
    }
    public override string Comment {
      get {
        IName DataWithComment = _Data as IName;
        if ( DataWithComment is not null ) {
          return DataWithComment.Comment;
        }
        return null;
      }

      set {
        IName DataWithComment = _Data as IName;
        if ( DataWithComment != null ) {
          DataWithComment.Comment = value;
        }
      }
    }

    #region IParent
    public AVMBase ParentVM {
      get {
        return Parent as AVMBase;
      }
    }
    #endregion IParent

    #region Support contact
    public virtual ISupportContact[] SupportContactData {
      get {
        if ( _Data is TSupportContactCollection) {
          return (_Data as TSupportContactCollection).Items.ToArray();
        }
        return null;
      }
    }
    public virtual bool SupportContactEnabler => SupportContactData != null;
    public Visibility SupportContactVisibility => SupportContactData != null ? Visibility.Visible : Visibility.Collapsed;
    public virtual string SupportContactIcon => App.GetPictureFullname("help");
    #endregion Support contact

    #region Refresh
    public virtual bool RefreshEnabler => !WorkInProgress;
    public virtual string RefreshIcon => App.GetPictureFullname("refresh");
    #endregion Refresh

    #region Filter
    public virtual string Filter {
      get {
        return _Filter;
      }
      set {
        _Filter = value;
        NotifyPropertyChanged(nameof(Filter));
      }
    }
    protected string _Filter;
    #endregion Filter

    #region Public properties

    public virtual AVMBase SelectedItem {
      get {
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
      }
    }
    private AVMBase _SelectedItem;

    public ObservableCollection<AVMBase> Items { get; set; } = new ObservableCollection<AVMBase>();

    public virtual string FullName {
      get {
        return Name;
      }
    }

    public string Message {
      get {
        return _Message;
      }

      set {
        if ( value != _Message ) {
          _Message = value;
          NotifyPropertyChanged(nameof(Message));
        }
      }
    }
    protected string _Message;

    public void ClearMessage(bool recurse = true) {
      Message = "";
      if ( recurse ) {
        foreach ( AVMBase InnerRowItem in Items ) {
          InnerRowItem.ClearMessage(recurse);
        }
      }
    }

    public override bool WorkInProgress {
      set {
        if ( value != base.WorkInProgress ) {
          base.WorkInProgress = value;
          NotifyPropertyChanged(nameof(RefreshEnabler));
          foreach ( AVMBase ItemItem in Items ) {
            ItemItem.WorkInProgress = value;
          }
        }
      }
    }
    public bool WorkNotInProgress => !WorkInProgress;

    public virtual string Picture {
      get {
        return App.GetPictureFullname("default");
      }
    }
    #endregion Public properties

    #region Commands
    public TRelayCommand CommandDisplaySupportContact { get; set; }
    public TRelayCommand CommandRefresh { get; set; }
    public TRelayCommand CommandClearFilter { get; set; }
    #endregion Commands

    #region Constructor(s)
    protected AVMBase() {
      Initialize();
    }
    protected AVMBase(object data) : base() {
      _Data = data;

      if ( data is IName NamedData ) {
        //Name = NamedData.Name;
        Description = NamedData.Description;
        Comment = NamedData.Comment;
      }

      Initialize();

    }

    protected override void Dispose(bool disposing) {
      base.Dispose(disposing);
      if ( !disposing ) {
        for ( int i = 0; i < Items.Count; i++ ) {
          Items[i].Dispose();
        }
        Items.Clear();
      }
    }

    protected virtual void Initialize() {
      _InitializeCommands();
    }
    protected override void _InitializeCommands() {
      CommandClearFilter = new TRelayCommand(() => { Filter = ""; }, _ => !string.IsNullOrWhiteSpace(Filter));
      //CommandDisplaySupportContact = new TRelayCommand(() => {
      //  if ( _Data is ISupportContactContainer SupportContactData && SupportContactData != null ) {
      //    SupportContactData.DisplaySupportMessage();
      //  }
      //},
      //  _ => { return true; }
      //);
    }
    #endregion Constructor(s)

    #region Converters
    public override string ToString() {
      StringBuilder RetVal = new();
      RetVal.Append(string.Format("Type: {0}, Name: {1}, Items.Count()={2}", GetType().Name, Name, Items.Count()));
      return RetVal.ToString();
    }
    #endregion Converters

    #region IEquatable<TDisplayRow>
    public bool Equals(AVMBase other) {
      return this.ToString() == other.ToString();
    }
    #endregion IEquatable<TDisplayRow>

    #region Public methods

    public virtual void AddItem(AVMBase baseItem) {
      baseItem.Parent = this;
      Items.Add(baseItem);
    }
    #endregion Public methods

  }
}
