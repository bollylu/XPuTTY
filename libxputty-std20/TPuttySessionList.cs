using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionList {

    #region --- Public properties ------------------------------------------------------------------------------
    public List<IPuttySession> Content { get; } = new List<IPuttySession>();
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region Private variables
    private readonly object Lock_Content = new object(); 
    #endregion Private variables

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionList() { }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      if ( Content.Any() ) {
        RetVal.AppendLine($"List of sessions ({Content.Count})");
        foreach ( IPuttySession SessionItem in Content ) {
          RetVal.AppendLine($"  {SessionItem.ToString()}");
        }
      } else {
        RetVal.Append("No session available");
      }
      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Indexers --------------------------------------------
    public IPuttySession this[int index] {
      get {
        lock ( Lock_Content ) {
          return Content[index];
        }
      }
    }

    public IPuttySession this[string sessionName] {
      get {
        lock ( Lock_Content ) {
          return Content.FirstOrDefault(x => x.Name == sessionName);
        }
      }
    }
    #endregion --- Indexers --------------------------------------------

    public IEnumerable<string> GetSessionNamesFromRegistry() {

      using ( RegistryKey BaseKey = Registry.CurrentUser.OpenSubKey(TPuttySession.REG_BASE) ) {
        IEnumerable<string> Sessions = BaseKey.GetSubKeyNames();
        return Sessions;
      }

    }

    public void ReadSessionsFromRegistry() {

      IEnumerable<string> Sessions = GetSessionNamesFromRegistry();
      if ( !Sessions.Any() ) {
        return;
      }

      lock ( Lock_Content ) {
        Content.Clear();
        foreach ( string SessionItem in Sessions ) {
          Content.Add(TPuttySession.GetSessionFromRegistry(SessionItem));
        }
      }

    }

    public void Clear() {
      lock ( Lock_Content ) {
        Content.Clear();
      }
    }

  }
}
