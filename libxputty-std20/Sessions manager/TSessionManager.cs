using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BLTools;

namespace libxputty_std20 {
  public class TSessionManager : ISessionManager {

    protected const char SPLIT_CHAR = ';';

    protected IDictionary<int, string> Sessions = new Dictionary<int, string>();

    public string StorageLocation { get; protected set; } = "";

    public TSessionManager() {
      _Load();
    }

    public TSessionManager(string storageLocation) {
      StorageLocation = storageLocation;
    }

    protected async void _Save() {
      if ( StorageLocation == "" ) {
        return;
      }

      try {
        using ( StreamWriter SessionManagerStream = File.CreateText(StorageLocation) ) {
          foreach ( KeyValuePair<int, string> SessionItem in Sessions ) {
            await SessionManagerStream.WriteAsync(SessionItem.Key.ToString());
            await SessionManagerStream.WriteAsync(SPLIT_CHAR);
            await SessionManagerStream.WriteLineAsync(SessionItem.Value);
          }
          await SessionManagerStream.FlushAsync();
          SessionManagerStream.Close();
        }

      } catch ( Exception ex ) {
        Log.Write($"Unable to save data to {StorageLocation} : {ex.Message}");
        return;
      }
    }

    protected void _Load() {
      if ( StorageLocation == "" ) {
        return;
      }

      try {
        Sessions.Clear();
        foreach ( string DictPairItem in File.ReadLines(StorageLocation) ) {
          int PID = int.Parse(DictPairItem.Before(SPLIT_CHAR));
          string Tag = DictPairItem.After(SPLIT_CHAR);
          Sessions.Add(PID, Tag);
        }
      } catch ( Exception ex ) {
        Log.Write($"Unable to load data from {StorageLocation} : {ex.Message}");
        return;
      }
    }

    public void AddSession(int PID, string tag) {
      try {
        Sessions.Add(PID, tag);
      } catch ( Exception ex ) {
        Log.Write($"Unable to add ({PID},{tag}) to the list of sessions : {ex.Message}");
      }
      _Save();
    }

    public int GetSession(string tag) {
      try {
        return Sessions.First(x => x.Value == tag).Key;
      } catch ( Exception ex ) {
        Log.Write($"Unable to get PID for session {tag} : {ex.Message}");
        return -1;
      }
    }

    public string GetSession(int PID) {
      return Sessions.TryGetValue(PID, out string RetVal) ? RetVal : "";
    }

    public IEnumerable<(int, string)> GetSessions() {
      foreach ( KeyValuePair<int, string> SessionItem in Sessions ) {
        yield return (SessionItem.Key, SessionItem.Value);
      }
    }

    public bool IsSessionRunning(int PID) {
      return Sessions.ContainsKey(PID);
    }

    public void RemoveSession(int PID) {
      try {
        Sessions.Remove(PID);
      } catch ( Exception ex ) {
        Log.Write($"Unable to remove {PID} to the list of sessions : {ex.Message}");
      }
      _Save();
    }

    public void RemoveSession(string tag) {
      int PID = 0;
      try {
        PID = GetSession(tag);
        Sessions.Add(PID, tag);
      } catch ( Exception ex ) {
        Log.Write($"Unable to add ({PID},{tag}) to the list of sessions : {ex.Message}");
      }
      _Save();
    }
  }
}
