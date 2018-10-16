using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionList {

    public List<TPuttySession> Content { get; } = new List<TPuttySession>();

    private readonly object Lock_Content = new object();

    public TPuttySessionList() { }


    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      if ( Content.Any() ) {
        RetVal.AppendLine($"List of sessions ({Content.Count})");
        foreach ( TPuttySession SessionItem in Content ) {
          RetVal.AppendLine($"  {SessionItem.ToString()}");
        }
      } else {
        RetVal.Append("No session available");
      }
      return RetVal.ToString();
    }

    public TPuttySession this[int index] {
      get {
        lock ( Lock_Content ) {
          return Content[index];
        }
      }
    }

    public IEnumerable<string> GetSessionListFromRegistry() {

      using ( RegistryKey BaseKey = Registry.CurrentUser.OpenSubKey(TPuttySession.REG_BASE) ) {
        IEnumerable<string> Sessions = BaseKey.GetSubKeyNames();
        return Sessions;
      }

    }

    public void ReadFromRegistry() {

      IEnumerable<string> Sessions = GetSessionListFromRegistry();
      if ( !Sessions.Any() ) {
        return;
      }

      lock ( Lock_Content ) {
        Content.Clear();
        foreach ( string SessionItem in Sessions ) {
          TPuttySession NewSession = new TPuttySession();
          NewSession.ReadFromRegistry(SessionItem);
          Content.Add(NewSession);
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
