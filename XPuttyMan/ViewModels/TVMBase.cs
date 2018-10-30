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
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMBase : TVMEasyPuttyBase, IEquatable<TVMBase> {


    public override ICredential Credential {
      get {
        if ( _Data is ICredentialContainer ) {
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
        if ( DataWithComment != null ) {
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
    public TVMBase ParentVM {
      get {
        return Parent as TVMBase;
      }
    }
    #endregion IParent

    #region Support contact
    public virtual TSupportContactCollection SupportContactData {
      get {
        if ( _Data is ISupportContactContainer ) {
          return (_Data as ISupportContactContainer).SupportContacts;
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

    public virtual TVMBase SelectedItem {
      get {
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
      }
    }
    private TVMBase _SelectedItem;

    public ObservableCollection<TVMBase> Items { get; set; } = new ObservableCollection<TVMBase>();

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
        foreach ( TVMBase InnerRowItem in Items ) {
          InnerRowItem.ClearMessage(recurse);
        }
      }
    }

    public override bool WorkInProgress {
      set {
        if ( value != _WorkInProgress ) {
          base.WorkInProgress = value;
          NotifyPropertyChanged(nameof(RefreshEnabler));
          foreach ( TVMBase ItemItem in Items ) {
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
    public TVMBase() {
      Initialize();
    }
    public TVMBase(object data) : base() {
      _Data = data;

      if ( data is IName NamedData ) {
        Name = NamedData.Name;
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

    protected override void Initialize() {
      base.Initialize();
      InitializeCommands();
    }
    protected virtual void InitializeCommands() {
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
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append(string.Format("Type: {0}, Name: {1}, Items.Count()={2}", GetType().Name, Name, Items.Count()));
      return RetVal.ToString();
    }
    public override XElement ToXml(string name) {
      return base.ToXml(name);
    }
    public override XElement ToXml() {
      throw new NotImplementedException();
    }
    #endregion Converters

    #region IEquatable<TDisplayRow>
    public bool Equals(TVMBase other) {
      return this.ToString() == other.ToString();
    }
    #endregion IEquatable<TDisplayRow>

    #region Public methods

    public virtual void AddItem(TVMBase baseItem) {
      baseItem.Parent = this;
      Items.Add(baseItem);
    }
    #endregion Public methods

  }
}
