using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools;

namespace libxputty_std20 {
  public abstract class TSessionManager : ISessionManager {

    protected IDictionary<int, string> Sessions = new Dictionary<int, string>();

    public string StorageLocation { get; protected set; } = "";

    protected readonly object _Lock = new object();

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      foreach ( KeyValuePair<int, string> SessionItem in Sessions ) {
        RetVal.AppendLine($"{SessionItem.Key};{SessionItem.Value}");
      }
      return RetVal.ToString();
    }

    public virtual void AddSession(int PID, string tag) {
      lock ( _Lock ) {
        try {
          Sessions.Add(PID, tag);
        } catch ( Exception ex ) {
          Log.Write($"Unable to add ({PID},{tag}) to the list of sessions : {ex.Message}");
        }
        _Save();
      }
    }

    public KeyValuePair<int, string> GetSession(string tag) {
      lock ( _Lock ) {
        try {
          return new KeyValuePair<int, string>(Sessions.First(x => x.Value == tag).Key, tag);
        } catch ( Exception ex ) {
          Log.Write($"Unable to get PID for session {tag} : {ex.Message}");
          return new KeyValuePair<int, string>(-1, "");
        }
      }
    }

    public KeyValuePair<int, string> GetSession(int PID) {
      lock ( _Lock ) {
        return Sessions.TryGetValue(PID, out string RetVal) ? new KeyValuePair<int, string>(PID, RetVal) : new KeyValuePair<int, string>(-1, "");
      }
    }

    public IEnumerable<KeyValuePair<int, string>> GetSessions() {
      lock ( _Lock ) {
        foreach ( KeyValuePair<int, string> SessionItem in Sessions ) {
          yield return SessionItem;
        }
      }
    }

    public bool IsSessionRunning(int PID) {
      return Sessions.ContainsKey(PID);
    }

    public virtual void RemoveSession(int PID) {
      lock ( _Lock ) {
        try {
          Sessions.Remove(PID);
        } catch ( Exception ex ) {
          Log.Write($"Unable to remove {PID} to the list of sessions : {ex.Message}");
        }
        _Save();
      }
    }

    public virtual void RemoveSession(string tag) {
      lock ( _Lock ) {
        int PID = 0;
        try {
          Sessions.Add(GetSession(tag).Key, tag);
        } catch ( Exception ex ) {
          Log.Write($"Unable to add ({PID},{tag}) to the list of sessions : {ex.Message}");
        }
        _Save();
      }
    }

    public virtual void Clear() {
      lock ( _Lock ) {
        Sessions.Clear();
        _Reset();
      }
    }

    protected abstract void _Save();
    protected abstract void _Load();
    protected abstract void _Reset();


    public static ISessionManager DEFAULT_SESSION_MANAGER {
      get {
        if ( _DEFAULT_SESSION_MANAGER == null ) {
          _DEFAULT_SESSION_MANAGER = new TSessionManagerMemory();
        }
        return _DEFAULT_SESSION_MANAGER;
      }
    }
    private static ISessionManager _DEFAULT_SESSION_MANAGER;
  }
}
