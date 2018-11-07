using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using BLTools;
using BLTools.Json;
using libxputty_std20.Interfaces;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionList : IToJson, IPuttySessionsList {

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
      foreach ( IPuttySession PuttySessionItem in sessions ) {
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

    public IJsonValue ToJson() {
      JsonArray Content = new JsonArray();
      foreach ( IPuttySession PuttySessionItem in Items ) {
        Content.Add(PuttySessionItem.ToJson());
      }
      JsonObject RetVal = new JsonObject() {
        //{JSON_THIS_ELEMENT, Content }
      };
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

    public void Add(IEnumerable<IPuttySession> sessions) {
      lock ( Lock_Content ) {
        foreach ( IPuttySession PuttySessionItem in sessions ) {
          Items.Add(PuttySessionItem);
        }
      }
    }

    #region --- Json --------------------------------------------
    public void SaveToJson(string filename) {
      if ( string.IsNullOrWhiteSpace(filename) ) {
        return;
      }

      try {
        File.WriteAllText(filename, this.ToJson().RenderAsString(true, 2));
      } catch ( Exception ex ) {
        Log.Write($"Unable to export sessions in JSON file {filename} : {ex.Message}");
      }

    }

    public string SaveToJsonString() {
      return this.ToJson().RenderAsString(true, 2);
    }

    public Stream SaveToJsonStream() {
      return this.ToJson().RenderAsString(true, 2).ToStream();
    }
    #endregion --- Json --------------------------------------------

    public static TPuttySessionList Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TPuttySessionList();
        }
        return _Empty;
      }
    }
    private static TPuttySessionList _Empty;
  }
}
