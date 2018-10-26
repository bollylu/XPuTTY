using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using BLTools;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionList {

    internal const string XML_THIS_ELEMENT = "Sessions";

    #region --- Public properties ------------------------------------------------------------------------------
    public List<IPuttySession> Items { get; } = new List<IPuttySession>();

    public int Count => Items.Count;

    #endregion --- Public properties ---------------------------------------------------------------------------

    #region Private variables
    private readonly object Lock_Content = new object();
    #endregion Private variables

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionList() { }
    public TPuttySessionList(IEnumerable<IPuttySession> sessions) {
      foreach(IPuttySession PuttySessionItem in sessions) {
        Add(PuttySessionItem);
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      if ( Items.Any() ) {
        RetVal.AppendLine($"List of sessions ({Items.Count})");
        foreach ( IPuttySession SessionItem in Items ) {
          RetVal.AppendLine($"  {SessionItem.ToString()}");
        }
      } else {
        RetVal.Append("No session available");
      }
      return RetVal.ToString();
    }


    public XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);

      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Indexers --------------------------------------------
    public IPuttySession this[int index] {
      get {
        lock ( Lock_Content ) {
          return Items[index];
        }
      }
    }

    public IPuttySession this[string sessionName] {
      get {
        lock ( Lock_Content ) {
          return Items.FirstOrDefault(x => x.Name == sessionName);
        }
      }
    }
    #endregion --- Indexers --------------------------------------------

    public IEnumerable<(string SessionName, string SessionProtocol)> GetSessionNamesAndProtocolFromRegistry() {
      using ( RegistryKey BaseKey = Registry.CurrentUser.OpenSubKey(TPuttySession.REG_KEYNAME_BASE) ) {
        foreach ( string KeyNameItem in BaseKey.GetSubKeyNames() ) {
          string Protocol = (BaseKey.OpenSubKey(KeyNameItem).GetValue(TPuttySession.REG_PROTOCOL_TYPE, "Unknown") as string).ToUpper();
          yield return (SessionName: KeyNameItem, SessionProtocol: Protocol);
        }
      }
      yield break;
    }

    public void ReadSessionsFromRegistry() {

      IEnumerable<(string, string)> SessionsWithProtocol = GetSessionNamesAndProtocolFromRegistry();
      if ( !SessionsWithProtocol.Any() ) {
        return;
      }

      lock ( Lock_Content ) {
        Items.Clear();
        foreach ( (string SessionName, string SessionProtocol) SessionItem in SessionsWithProtocol ) {

          IPuttySession NewSession;
          if ( SessionItem.SessionProtocol == TPuttyProtocol.SSH ) {
            NewSession = new TPuttySessionSSH(SessionItem.SessionName);
            Items.Add(NewSession.LoadFromRegistry());
            continue;
          }
          if ( SessionItem.SessionProtocol == TPuttyProtocol.Serial ) {
            NewSession = new TPuttySessionSerial(SessionItem.SessionName);
            Items.Add(NewSession.LoadFromRegistry());
            continue;
          }
          if ( SessionItem.SessionProtocol == TPuttyProtocol.Telnet ) {
            NewSession = new TPuttySessionTelnet(SessionItem.SessionName);
            Items.Add(NewSession.LoadFromRegistry());
            continue;
          }
          if ( SessionItem.SessionProtocol == TPuttyProtocol.RLogin ) {
            NewSession = new TPuttySessionRLogin(SessionItem.SessionName);
            Items.Add(NewSession.LoadFromRegistry());
            continue;
          }
          if ( SessionItem.SessionProtocol == TPuttyProtocol.Raw ) {
            NewSession = new TPuttySessionRaw(SessionItem.SessionName);
            Items.Add(NewSession.LoadFromRegistry());
            continue;
          }
        }

      }

    }

    public void Clear() {
      lock ( Lock_Content ) {
        Items.Clear();
      }
    }

    public void Add(IPuttySession session) {
      lock ( Lock_Content ) {
        Items.Add(session);
      }
    }

    public void Export(string filename) {
      if (string.IsNullOrWhiteSpace(filename)) {
        return;
      }

      XDocument ExportFile = new XDocument(new XDeclaration("1.0", Encoding.UTF8.EncodingName, "yes"));
      XElement Root = new XElement(XML_THIS_ELEMENT);
      foreach(IPuttySession PuttySessionItem in Items) {
        Root.Add(PuttySessionItem.ToXml());
      }
      ExportFile.Add(Root);
      try {
        ExportFile.Save(filename);
      } catch (Exception ex) {
        Log.Write($"Unable to export sessions in XML file {filename} : {ex.Message}");
      }

    }
  }
}
