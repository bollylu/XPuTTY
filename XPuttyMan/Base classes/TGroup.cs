using EasyPutty.Base;
using BLTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XPuttyMan {
  public class TGroup : TSupportContactContainerBase, ICredentialContainer {

    #region XML constants
    public static XName XML_THIS_ELEMENT {
      get {
        return GetXName("Group");
      }
    }
    #endregion XML constants

    public TGroup ParentGroup {
      get {
        return GetParent<TGroup>();
      }
    }

    public List<object> Items { get; protected set; } = new List<object>();

    public TAlertTriggerCollection AlertTriggers {
      get {
        if (_AlertTriggers != null) {
          return _AlertTriggers;
        }
        if (ParentGroup != null) {
          return ParentGroup.AlertTriggers;
        }
        return null;
      }
    }
    protected TAlertTriggerCollection _AlertTriggers;

    public TReportCollection Reports {
      get {
        return _Reports;
      }
    }
    protected TReportCollection _Reports;

    #region Constructor(s)
    public TGroup() : base() {
      Initialize();
    }
    public TGroup(string name) : base() {
      Name = name;
      Initialize();
    }
    public TGroup(string name, IEnumerable<object> items) : base() {
      Name = name;
      if (items == null) {
        Initialize();
        return;
      }
      Initialize();
      foreach (object ItemItem in items) {
        AddItem(ItemItem);
      }
    }

    public TGroup(TGroup group) : base(group) {
      foreach (object ItemItem in group.Items) {
        if (ItemItem is TGroup) {
          AddItem(new TGroup(ItemItem as TGroup));
          continue;
        }
        if (ItemItem is TComputer) {
          AddItem(new TComputer(ItemItem as TComputer));
          continue;
        }
        if (ItemItem is TCluster) {
          AddItem(new TComputer(ItemItem as TComputer));
          continue;
        }
      }
      if (group._AlertTriggers != null) {
        _AlertTriggers = new TAlertTriggerCollection(group._AlertTriggers);
      }
      if (group._Reports != null) {
        _Reports = new TReportCollection(group._Reports);
      }
      Parent = group.Parent;
    }
    public TGroup(XElement group) : base(group) {

      #region Validate parameters
      if (group == null || !group.HasAttributes) {
        Initialize();
        return;
      } 
      #endregion Validate parameters

      Initialize();

      if (group.Elements(TAlertTriggerCollection.XML_THIS_ELEMENT).Count() > 0) {
        _AlertTriggers = new TAlertTriggerCollection(group.SafeReadElement(TAlertTriggerCollection.XML_THIS_ELEMENT));
        _AlertTriggers.Parent = this;
      }

      if (group.Elements(TReportCollection.XML_THIS_ELEMENT).Count() > 0) {
        _Reports = new TReportCollection(group.SafeReadElement(TReportCollection.XML_THIS_ELEMENT));
        _Reports.Parent = this;
      }

      foreach (XElement SubItem in group.Elements()) {

        if (SubItem.Name == TComputer.XML_THIS_ELEMENT) {
          TComputer NewComputer = new TComputer(SubItem);
          AddItem(NewComputer);
          continue;
        }

        if (SubItem.Name == TCluster.XML_THIS_ELEMENT) {
          AddItem(new TCluster(SubItem));
          continue;
        }

        if (SubItem.Name == TComputerRDS.XML_THIS_ELEMENT) {
          AddItem(new TComputerRDS(SubItem));
          continue;
        }

        if (SubItem.Name == TGroup.XML_THIS_ELEMENT) {
          AddItem(new TGroup(SubItem));
          continue;
        }

      }
    }

    protected override void Dispose(bool disposing) {
      _AlertTriggers.Dispose();
      base.Dispose(disposing);
    }

    #endregion Constructor(s)

    public void AddItem(object item) {
      if (item is IParent) {
        IParent ItemWithParent = item as IParent;
        ItemWithParent.Parent = this;
      }
      Items.Add(item);
    }

    #region Converters
    public override XElement ToXml() {
      NotifyExecutionProgress($"Converting TGroup {Name} ToXml");
      Trace.Indent();
      XElement RetVal = base.ToXml(XML_THIS_ELEMENT);

      if (AlertTriggers != null) {
        RetVal.Add(AlertTriggers.ToXml());
      }

      if (Reports != null) {
        RetVal.Add(Reports.ToXml());
      }

      foreach (IToXml ItemItem in Items.Where(x => x is IToXml)) {
        RetVal.Add(ItemItem.ToXml());
      }
      Trace.Unindent();
      return RetVal;
    }


    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Group {Name}");
      RetVal.Append($", {Items.Count} items");
      return RetVal.ToString();
    }
    #endregion Converters

  }
}
