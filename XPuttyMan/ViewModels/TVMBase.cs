using EasyPutty.Base;
using BLTools;
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

namespace ServicesManager {
  public class TVMBase : TEasyPuttyBase, IEquatable<TVMBase>, IStatus {

    /// <summary>
    /// Set this to true to view additional debug message
    /// </summary>
    public static bool IsDebug = false;

    protected object _Data { get; set; }
    protected object _Lock = new object();

    public override ICredential Credential {
      get {
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
        if (DataWithDescription != null) {
          return DataWithDescription.Description;
        }
        return null;
      }

      set {
        IName DataWithDescription = _Data as IName;
        if (DataWithDescription != null) {
          DataWithDescription.Description = value;
        }
      }
    }
    public override string Comment {
      get {
        IName DataWithComment = _Data as IName;
        if (DataWithComment != null) {
          return DataWithComment.Comment;
        }
        return null;
      }

      set {
        IName DataWithComment = _Data as IName;
        if (DataWithComment != null) {
          DataWithComment.Comment = value;
        }
      }
    }

    #region IStatus
    /// <summary>
    /// Current status of the item
    /// </summary>
    public virtual string Status {
      get {
        return _Status;
      }
      set {
        if (_Status != value) {
          _Status = value;
        }
        if (OnStatusChanged != null) {
          OnStatusChanged(this, new StringEventArgs(Status));
        }
        NotifyPropertyChanged(nameof(Status));
        NotifyPropertyChanged(nameof(DisplayStatus));
        NotifyPropertyChanged(nameof(RefreshEnabler));
        NotifyPropertyChanged(nameof(RestartEnabler));
        NotifyPropertyChanged(nameof(DisplayShares));
      }
    }
    protected string _Status;

    public event EventHandler<StringEventArgs> OnStatusChanged;
    public bool HasStatus {
      get {
        return !string.IsNullOrWhiteSpace(Status);
      }
    }
    public void ClearStatus(bool recurse = true) {
      Status = "";
      if (OnStatusChanged != null) {
        OnStatusChanged(this, new StringEventArgs(Status));
      }
      if (recurse) {
        foreach (TVMBase ItemItem in Items) {
          ItemItem.ClearStatus(recurse);
        }
      }
    }

    public string DisplayStatus {
      get {
        if (string.IsNullOrWhiteSpace(Status)) {
          return "";
        }
        return string.Format(" ({0})", Status);
      }
    }
    #endregion IStatus

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
        if (_Data is ISupportContactContainer) {
          return (_Data as ISupportContactContainer).SupportContacts;
        }
        return null;
      }
    }
    public virtual bool SupportContactEnabler {
      get {
        return SupportContactData != null;
      }
    }
    public Visibility SupportContactVisibility {
      get {
        return SupportContactData != null ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    public virtual string SupportContactIcon {
      get {
        return App.GetPictureFullname("help");
      }
    }
    #endregion Support contact

    #region Refresh
    public virtual bool RefreshEnabler {
      get {
        if (WorkInProgress) {
          return false;
        }
        return true;
      }
    }
    public virtual string RefreshIcon {
      get {
        return App.GetPictureFullname("refresh");
      }
    }
    #endregion Refresh

    #region Restart
    protected IRestartable RestartableData {
      get {
        if (_Data is IRestartable) {
          return _Data as IRestartable;
        }
        return null;
      }
    }
    public TRestartChainList RestartChainList {
      get {
        if (_RestartChainList == null) {
          _RestartChainList = new TRestartChainList();
        }
        if (_RestartChainList.Count() == 0) {
          _RestartChainList.AddItem(this);

          try {
            //if (IsDebug) {
            //  Trace.WriteLine(string.Format("GetItemsInRestartChain for {0}", _Data.Name));
            //  Trace.Indent();
            //}

            if (RestartableData != null && RestartableData.RestartAuthorization != null) {
              foreach (TRestartChainItem ChainItemItem in RestartableData.RestartAuthorization.RestartChain) {
                TVMBase NextRestart = FindDisplayRowInGroup(ChainItemItem);
                if (NextRestart != null) {
                  if (!_RestartChainList.AddItem(NextRestart)) {
                    return _RestartChainList;
                  }
                }
              }
            }
          } finally {
            if (IsDebug) {
              Trace.Unindent();
            }
          }
        }
        return _RestartChainList;
      }
    }
    private TRestartChainList _RestartChainList = new TRestartChainList();
    public string FullRestartChainNames {
      get {
        if (RestartChainList.Count == 0) {
          return null;
        }
        return string.Join("\n", RestartChainList.Select(x => x.ToString()));
      }
    }

    public virtual bool RestartEnabler {
      get {
        if (!(_Data is IRestartable)) {
          return false;
        }
        if (RestartableData == null || RestartableData.RestartAuthorization == null) {
          return false;
        }
        if (WorkInProgress) {
          return false;
        }
        if (!HasStatus) {
          return false;
        }
        return RestartableData.RestartAuthorization.IsRestartable;
      }
    }
    public Visibility RestartVisibilityInMenu {
      get {
        if (RestartableData == null || RestartableData.RestartAuthorization == null) {
          return Visibility.Collapsed;
        }
        return RestartableData.RestartAuthorization.IsRestartable ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    public Visibility RestartableVisibility {
      get {
        if (RestartableData == null || RestartableData.RestartAuthorization == null) {
          return Visibility.Hidden;
        }
        return RestartableData.RestartAuthorization.IsRestartable ? Visibility.Visible : Visibility.Hidden;
      }
    }

    public virtual string RestartIcon {
      get {
        return App.GetPictureFullname("restart");
      }
    }
    #endregion Restart

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

    #region ContentModified
    public event EventHandler OnContentModified;
    public void NotifyContentModified() {
      if (OnContentModified != null) {
        OnContentModified(this, EventArgs.Empty);
      }
    }
    protected virtual async void ContentModifiedAction(object s, EventArgs e) {
      await Task.Yield();
      GetDisplayDetails();
      NotifyContentModified();
    }
    #endregion ContentModified

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

    protected string _Message;
    public string Message {
      get {
        return _Message;
      }

      set {
        if (value != _Message) {
          _Message = value;
          NotifyPropertyChanged(nameof(Message));
        }
      }
    }

    public void ClearMessage(bool recurse = true) {
      Message = "";
      if (recurse) {
        foreach (TVMBase InnerRowItem in Items) {
          InnerRowItem.ClearMessage(recurse);
        }
      }
    }

    public override bool WorkInProgress {
      set {
        if (value != _WorkInProgress) {
          base.WorkInProgress = value;
          NotifyPropertyChanged(nameof(RefreshEnabler));
          NotifyPropertyChanged(nameof(RestartEnabler));
          foreach (TVMBase ItemItem in Items) {
            ItemItem.WorkInProgress = value;
          }
        }
      }
    }
    public bool WorkNotInProgress {
      get {
        return !WorkInProgress;
      }
    }

    public TRestartAuthorization RestartAuthorization {
      get {
        if (RestartableData != null) {
          return RestartableData.RestartAuthorization;
        } else {
          return null;
        }
      }
    }

    public virtual string Picture {
      get {
        return App.GetPictureFullname(this.GetType().Name);
      }
    }

    public virtual bool IsAdvancedFeature {
      get {
        return false;
      }
    }
    #endregion Public properties

    #region Views
    public ObservableCollection<TView> DetailViews { get; set; } = new ObservableCollection<TView>();

    public int SelectedDetailViewIndex {
      get {
        if (DetailViews == null || DetailViews.Count == 0) {
          return -1;
        }
        return _SelectedDetailViewIndex;
      }
      set {
        if (DetailViews != null && DetailViews.Count > 0) {
          _SelectedDetailViewIndex = value;
          NotifyPropertyChanged(nameof(SelectedDetailViewIndex));
        }
      }
    }
    protected int _SelectedDetailViewIndex;

    public virtual Visibility TabSessionRDSVisibility {
      get {
        return (this is TVMServerRDS) ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    public virtual Visibility TabSharesVisibility {
      get {
        if (this is TVMServer) {
          return Visibility.Visible;
        }
        if (this is TVMClusterResource) {
          TVMClusterResource CurrentResource = this as TVMClusterResource;
        }
        return Visibility.Collapsed;
      }
    }

    public ObservableCollection<TVMDetailSection> DetailSections { get; set; } = new ObservableCollection<TVMDetailSection>();
    public ObservableCollection<TVMDiskSizeInfo> DiskSizeInfos { get; set; } = new ObservableCollection<TVMDiskSizeInfo>();

    public string DisplayShares {
      get {
        return _DisplayShares;
      }
      set {
        _DisplayShares = value;
        NotifyPropertyChanged(nameof(DisplayShares));
      }
    }
    private string _DisplayShares;
    #endregion Views

    #region Private and protected variables
    protected TVMGroup CurrentGroup {
      get {
        if (this is TVMGroup) {
          return this as TVMGroup;
        }
        if (this is TVMCluster || this is TVMServer) {
          return Parent as TVMGroup;
        }
        if (this is TVMClusterRole || this is TVMService) {
          return Parent.Parent as TVMGroup;
        }
        if (this is TVMClusterResource) {
          return Parent.Parent.Parent as TVMGroup;
        }
        return null;
      }
    }
    protected List<TVMBase> CurrentGroupFlatList {
      get {
        if (CurrentGroup == null) {
          return new List<TVMBase>();
        }
        IEnumerable<TVMBase> RestartableServers = CurrentGroup.Items
                                                                  .Where(x => x is TVMServer)
                                                                  .Where(x => x.RestartAuthorization.IsRestartable);
        IEnumerable<TVMBase> RestartableServices = CurrentGroup.Items
                                                                   .Where(x => x is TVMServer)
                                                                   .SelectMany(x => x.Items)
                                                                   .Where(x => x is TVMService)
                                                                   .Where(x => x.RestartAuthorization.IsRestartable);
        IEnumerable<TVMBase> RestartableRoles = CurrentGroup.Items
                                                                .Where(x => x is TVMCluster)
                                                                .SelectMany(x => x.Items)
                                                                .Where(x => x is TVMClusterRole)
                                                                .Where(x => x.RestartAuthorization.IsRestartable);

        IEnumerable<TVMBase> RestartableResources = CurrentGroup.Items
                                                                    .Where(x => x is TVMCluster)
                                                                    .SelectMany(x => x.Items)
                                                                    .Where(x => x is TVMClusterRole)
                                                                    .SelectMany(x => x.Items)
                                                                    .Where(x => x is TVMClusterResource)
                                                                    .Where(x => x.RestartAuthorization.IsRestartable);

        return RestartableServers.Union(RestartableServices)
                                  .Union(RestartableRoles)
                                  .Union(RestartableResources)
                                  .ToList();
      }
    }
    #endregion Private and protected variables

    public static event EventHandler<StringEventArgs> ConsoleNotification;
    public void NotifyConsole(string message) {
      if (ConsoleNotification != null) {
        ConsoleNotification(this, new StringEventArgs(message));
      }
    }

    #region Commands
    public TRelayCommand CommandDisplaySupportContact { get; set; }
    public TRelayCommand CommandRefresh { get; set; }
    public TRelayCommand CommandRestart { get; set; }
    public TRelayCommand CommandClearFilter { get; set; }
    public TRelayCommand CommandRefreshDiskSizeInfo { get; set; }
    #endregion Commands

    #region Constructor(s)
    public TVMBase() {
      Initialize();
    }
    public TVMBase(object data) : base() {
      _Data = data;

      if (data is IName) {
        IName NamedData = _Data as IName;
        Name = NamedData.Name;
        Description = NamedData.Description;
        Comment = NamedData.Comment;
      }

      if (_Data is IStatus) {
        IStatus RowWithStatus = _Data as IStatus;
        RowWithStatus.OnStatusChanged += RowWithStatus_OnStatusChanged;
      }

      if (_Data is IRestartable) {
        IRestartable RowRestartable = _Data as IRestartable;
        RowRestartable.RestartAuthorization.OnRestartableChanged += RestartAuthorization_OnRestartableChanged;
      }

      Initialize();

    }

    protected override void Dispose(bool disposing) {
      base.Dispose(disposing);
      if (!disposing) {
        for (int i = 0; i <= Items.Count; i++) {
          Items[i].OnContentModified -= ContentModifiedAction;
        }
        if (_Data != null) {
          if (_Data is IStatus) {
            IStatus RowWithStatus = _Data as IStatus;
            RowWithStatus.OnStatusChanged -= RowWithStatus_OnStatusChanged;
          }
          if (_Data is IRestartable) {
            IRestartable RowRestartable = _Data as IRestartable;
            RowRestartable.RestartAuthorization.OnRestartableChanged -= RestartAuthorization_OnRestartableChanged;
          }
        }
        for (int i = 0; i < Items.Count; i++) {
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
      CommandRefresh = new TRelayCommand(
        async () => {
          await Refresh();
        },
        _ => { return RefreshEnabler; }
      );

      CommandRestart = new TRelayCommand(
        async () => {
          await Restart();
        }
      );

      CommandClearFilter = new TRelayCommand(
        () => {
          Filter = "";
        },
        _ => { return !string.IsNullOrWhiteSpace(Filter); }
      );

      CommandRefreshDiskSizeInfo = new TRelayCommand(
        async () => {
          await GetDiskSizeInfos();
        },
        _ => { return true; }
      );

      CommandDisplaySupportContact = new TRelayCommand(
        () => {
          if (_Data is ISupportContactContainer) {
            ISupportContactContainer SupportContactData = _Data as ISupportContactContainer;
            if (SupportContactData != null) {
              TVMSupportContacts DisplaySupportContacts = new TVMSupportContacts(SupportContactData);
              DisplaySupportContacts.DisplaySupportMessage();
            }
          }
        },
        _ => { return true; }
      );
    }
    #endregion Constructor(s)

    #region Converters
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append(string.Format("Type: {0}, Name: {1}, Items.Count()={2}", GetType().Name, Name, Items.Count()));
      if (DetailViews.Count > 0) {
        RetVal.AppendFormat(", Views.Count()={0}", DetailViews.Count());
      }
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
      //baseItem.OnContentModified += ContentModifiedAction;
      Items.Add(baseItem);
    }

    public async virtual Task GetDetails() {
      await Task.Yield();
      GetDisplayDetails();
    }

    public async virtual Task Refresh() {

      try {
        WorkInProgress = true;
        if (this is IOnline) {
          IOnline TestableItem = this as IOnline;
          if (!await TestableItem.CheckOnline()) {
            return;
          }
          if (!await TestableItem.CheckPowerShellReady()) {
            return;
          }
        }

        await GetDetails();

        NotifyPropertyChanged(nameof(Status));
        NotifyPropertyChanged(nameof(DisplayStatus));
        NotifyPropertyChanged(nameof(DisplayShares));

        if (Items.Count > 0) {
          await Task.WhenAll(Items.Select(async x => await x.Refresh()));
        }
      } catch (Exception ex) {
        NotifyExecutionError($"Unable to refresh : {ex.Message}");
      } finally {
        WorkInProgress = false;
      }

    }

    public async virtual Task<bool> Restart(int timeout = 240, bool force = true) {
      return await Task.FromResult<bool>(true);
    }

    public virtual bool GetRestartConfirmation() {
      if (RestartableData == null) {
        return false;
      }
      if (!RestartableData.RestartAuthorization.Confirm()) {
        return false;
      }
      if (!RestartableData.RestartAuthorization.DoubleConfirm()) {
        return false;
      }
      return true;

    }

    public async virtual Task GetDiskSizeInfos() {
      await Task.Yield();
    }
    #endregion Public methods

    #region Private methods
    private void RestartAuthorization_OnRestartableChanged(object sender, BoolEventArgs e) {
      NotifyPropertyChanged(nameof(RestartEnabler));
      NotifyPropertyChanged(nameof(RestartableVisibility));
    }

    private TVMBase FindDisplayRowInGroup(TRestartChainItem chainItem) {

      if (chainItem.IsRestartClusterItem) {
        return CurrentGroupFlatList.Find(x => x is TVMCluster && x.Name.ToUpper() == chainItem.ClusterComponent);
      }
      if (chainItem.IsRestartClusterRoleItem) {
        return CurrentGroupFlatList.Where(x => x is TVMClusterRole)
                                   .Where(x => x.ParentVM.Name.ToUpper() == chainItem.ClusterComponent)
                                   .Where(x => x.Name.ToUpper() == chainItem.RoleComponent)
                                   .SingleOrDefault();
      }
      if (chainItem.IsRestartClusterResourceItem) {
        return CurrentGroupFlatList.Where(x => x is TVMClusterResource)
                                   .Where(x => x.ParentVM.ParentVM.Name.ToUpper() == chainItem.ClusterComponent)
                                   .Where(x => x.ParentVM.Name.ToUpper() == chainItem.RoleComponent)
                                   .Where(x => x.Name.ToUpper() == chainItem.ResourceComponent)
                                   .SingleOrDefault();
      }

      if (chainItem.IsRestartServerItem) {
        return CurrentGroupFlatList.Find(x => x is TVMServer && x.Name.ToUpper() == chainItem.ServerComponent);
      }
      if (chainItem.IsRestartServiceItem) {
        return CurrentGroupFlatList.Where(x => x is TVMService)
                                   .Where(x => x.ParentVM.Name.ToUpper() == chainItem.ServerComponent)
                                   .Where(x => x.Name.ToUpper() == chainItem.ServiceComponent)
                                   .SingleOrDefault();
      }

      return null;
    }

    protected override void NotifyExecutionProgress(string message = "", ErrorLevel errorlevel = ErrorLevel.Info) {
      base.NotifyExecutionProgress(message, errorlevel);
    }

    private void RowWithNotifications_OnActionExecutionProgress(object sender, IntAndMessageEventArgs e) {
      Message = e.Message;
      NotifyConsole(e.Message);
    }

    private void RowWithNotifications_OnExecutionError(object sender, StringEventArgs e) {
      Trace.WriteLine(e.Value);
      Message = e.Value;
      NotifyConsole(e.Value);
    }

    private void RowWithStatus_OnStatusChanged(object sender, StringEventArgs e) {
      Status = e.Value;
    }

    public virtual void GetDisplayDetails() {
      lock (_Lock) {
        DetailSections.Clear();
        AddDisplayDetailsComment();
        StringBuilder RetVal = new StringBuilder();
        RetVal.AppendLine(string.Format("Type : {0}", this.GetType().Name));
        RetVal.AppendLine(string.Format("Name : {0}", Name));
        if (!string.IsNullOrWhiteSpace(Description)) {
          RetVal.AppendLine(string.Format("Description : {0}", Description));
        }
        DetailSections.Add(new TVMDetailSection("Basic info", RetVal.ToString()));
        AddDisplayDetailsSubItems();
      }
    }

    public void AddDisplayDetailsSubItems() {
      if (Items != null && Items.Count() > 0) {
        StringBuilder Content = new StringBuilder();
        Content.AppendLine(string.Format("SubItems.Count() : {0}", Items.Count()));
        DetailSections.Add(new TVMDetailSection("SubItems", Content.ToString()));
      }
    }

    public void AddDisplayDetailsComment() {
      if (!string.IsNullOrWhiteSpace(Comment)) {
        DetailSections.Add(new TVMDetailSection("Comment", Comment));
      }
    }


    protected async virtual Task GetDisplayShares() {
      DisplayShares = "";
      await Task.Run(() => {
        StringBuilder RetVal = new StringBuilder();
        RetVal.AppendLine("List of shares");
        RetVal.AppendLine("--------------");
        RetVal.AppendLine();
        DisplayShares = RetVal.ToString();
      });
    }
    #endregion Private methods

    public class TRestartChainList : List<TVMBase> {

      public bool AddItem(TVMBase item) {
        if (this.Contains(item)) {
          return false;
        }
        this.Add(item);
        return true;
      }

      public bool AddItems(IEnumerable<TVMBase> items) {
        foreach (TVMBase ItemItem in items) {
          if (!this.AddItem(ItemItem)) {
            return false;
          }
        }
        return true;
      }

    }

  }
}
