using System;
using System.Collections.Generic;
using System.IO;

using BLTools;

namespace libxputty_std20 {
  public class TSessionManagerCSV : TSessionManager {

    protected const char SEPARATOR = ';';

    public TSessionManagerCSV() {
      _Load();
    }

    public TSessionManagerCSV(string storageLocation) {
      StorageLocation = storageLocation;
    }

    protected override async void _Save() {
      if ( StorageLocation == "" ) {
        Log.Write("Unable to save to empty storage location");
        return;
      }

      try {
        using ( StreamWriter SessionManagerStream = File.CreateText(StorageLocation) ) {
          SessionManagerStream.AutoFlush = true;
          foreach ( KeyValuePair<int, string> SessionItem in Sessions ) {
            await SessionManagerStream.WriteAsync($"{SessionItem.Key.ToString()};{SessionItem.Value}");
          }
          SessionManagerStream.Close();
        }

      } catch ( Exception ex ) {
        Log.Write($"Unable to save data to {StorageLocation} : {ex.Message}");
        return;
      }
    }

    protected override void _Load() {
      if ( StorageLocation == "" ) {
        return;
      }

      try {
        Sessions.Clear();
        foreach ( string DictPairItem in File.ReadLines(StorageLocation) ) {
          int PID = int.Parse(DictPairItem.Before(SEPARATOR));
          string Tag = DictPairItem.After(SEPARATOR);
          Sessions.Add(PID, Tag);
        }
      } catch ( Exception ex ) {
        Log.Write($"Unable to load data from {StorageLocation} : {ex.Message}");
        return;
      }
    }

    protected override void _Reset() {
      try {
        File.Delete(StorageLocation);
      } catch ( Exception ex ) {
        Log.Write($"Unable to clean up storage file {StorageLocation} : {ex.Message}");
      }
    }

  }
}
