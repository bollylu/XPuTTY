using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using BLTools;
using BLTools.Json;
using Microsoft.Win32;

namespace libxputty {
  public class TSessionList : ISessionList {

    #region --- Public properties ------------------------------------------------------------------------------
    
    public List<ISession> Items { get; } = new();
    private readonly object Lock_Content = new();

    public int Count => Items.Count;
    #endregion --- Public properties ---------------------------------------------------------------------------


    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TSessionList() { }
    public TSessionList(IEnumerable<ISession> sessions) {
      foreach ( ISession SessionItem in sessions ) {
        Add(SessionItem);
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new();
      if ( Items.Any() ) {
        RetVal.AppendLine($"List of sessions ({Items.Count})");
        foreach ( ISession SessionItem in Items ) {
          RetVal.AppendLine($"  {SessionItem}");
        }
      } else {
        RetVal.Append("No session available");
      }
      return RetVal.ToString();
    }

    public IJsonValue ToJson() {
      //JsonArray Content = new JsonArray();
      //foreach ( ISession SessionItem in Items ) {
      //  Content.Add(SessionItem.ToJson());
      //}
      //JsonObject RetVal = new JsonObject() {
      //  //{JSON_THIS_ELEMENT, Content }
      //};
      //return RetVal;
      return null;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Indexers --------------------------------------------
    public ISession this[int index] {
      get {
        lock ( Lock_Content ) {
          return Items[index];
        }
      }
    }

    public ISession this[string sessionName] {
      get {
        lock ( Lock_Content ) {
          return Items.FirstOrDefault(x => x.Name.Equals(sessionName, StringComparison.InvariantCultureIgnoreCase));
        }
      }
    }
    #endregion --- Indexers --------------------------------------------

    public void Clear() {
      lock ( Lock_Content ) {
        Items.Clear();
      }
    }

    public void Add(ISession session) {
      lock ( Lock_Content ) {
        Items.Add(session);
      }
    }

    public void Add(IEnumerable<ISession> sessions) {
      lock ( Lock_Content ) {
        foreach ( ISession PuttySessionItem in sessions ) {
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

    public static TSessionList Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TSessionList();
        }
        return _Empty;
      }
    }
    private static TSessionList _Empty;
  }
}
