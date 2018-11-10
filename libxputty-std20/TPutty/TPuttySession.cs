using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using BLTools;
using BLTools.Json;

using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySession : IPuttySession, IDisposable, ISupportContactContainer, IToJson {

    #region --- Constants --------------------------------------------
    public const string EXECUTABLE_PUTTY = "putty.exe";
    public static string EXECUTABLE_PUTTY_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PUTTY);
    public const string EXECUTABLE_PLINK = "plink.exe";
    public static string EXECUTABLE_PLINK_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PLINK);
    public const string EXECUTABLE_PSCP = "pscp.exe";

    public const string JSON_THIS_ELEMENT = "Session";
    public const string JSON_NAME = "Name";
    public const string JSON_PROTOCOL = "Protocol";
    #endregion --- Constants --------------------------------------------

    #region --- IName --------------------------------------------
    public string Name { get; set; }
    public string Description { get; set; }
    public string Comment { get; set; }
    #endregion --- IName --------------------------------------------

    #region --- Public properties ------------------------------------------------------------------------------
    public string CleanName => Name.Replace("%20", " ");
    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();

    public string RemoteCommand { get; set; }

    private IEnumerable<string> Groups => CleanName.ItemsBetween("[", "]");

    public string GroupLevel1 {
      get {
        if ( string.IsNullOrWhiteSpace(_GroupLevel1) && Groups.Any() ) {
          _GroupLevel1 = Groups.First();
        }
        return _GroupLevel1;
      }
      set {
        _GroupLevel1 = value;
      }
    }
    private string _GroupLevel1;

    public string GroupLevel2 {
      get {
        if ( string.IsNullOrWhiteSpace(_GroupLevel2) && Groups.Any() && Groups.Count() > 1 ) {
          _GroupLevel2 = Groups.Skip(1).First();
        }
        return _GroupLevel2;
      }
      set {
        _GroupLevel2 = value;
      }
    }
    private string _GroupLevel2;

    public string Section {
      get {
        if ( string.IsNullOrWhiteSpace(_Section) ) {
          _Section = CleanName.Between("{", "}");
        }
        return _Section;
      }
      set {
        _Section = value;
      }
    }
    private string _Section;
    #endregion --- Public properties ---------------------------------------------------------------------------

    protected string TempFileForRemoteCommand;

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");
      return RetVal.ToString();
    }

    public virtual IJsonValue ToJson() {
      JsonObject RetVal = new JsonObject {
        { JSON_THIS_ELEMENT, new JsonObject {
            { JSON_NAME, Json.JsonEncode(Name) },
            { JSON_PROTOCOL, Protocol }
          }
        }
      };
      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Event handlers --------------------------------------------
    public event EventHandler OnStart;
    public event EventHandler OnExit;
    #endregion --- Event handlers --------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySession() {
      Name = "<no name>";
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;

    }

    public TPuttySession(string name) {
      Name = name;
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;
    }

    public TPuttySession(IPuttySession session) {
      Name = session.Name;
      Description = session.Description;
      Comment = session.Comment;
      GroupLevel1 = session.GroupLevel1;
      GroupLevel2 = session.GroupLevel2;
      Section = session.Section;
      RemoteCommand = session.RemoteCommand;
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;
    }

    public virtual void Dispose() {
      PuttyProcess.OnExit -= PuttyProcess_OnExit;
      PuttyProcess.OnStart -= PuttyProcess_OnStart;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public static IPuttySession Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TPuttySession("<empty>");
        }
        return _Empty;
      }
    }
    private static IPuttySession _Empty;

    #region --- Windows processes --------------------------------------------
    public TRunProcess PuttyProcess { get; protected set; } = new TRunProcess();
    public bool IsRunning => PuttyProcess.IsRunning;
    public int PID => PuttyProcess.PID;
    public string CommandLine => TRunProcess.GetCommandLine(PID);

    public ISupportContact SupportContact => throw new NotImplementedException();

    public virtual void Start(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = arguments == "" ? $"{"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }

    public virtual void StartPlink(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = arguments == "" ? $"{"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }

    private void PuttyProcess_OnStart(object sender, EventArgs e) {
      if ( OnStart != null ) {
        OnStart(this, EventArgs.Empty);
      }
    }

    private void PuttyProcess_OnExit(object sender, EventArgs e) {
      if (!string.IsNullOrWhiteSpace(TempFileForRemoteCommand)) {
        Log.Write($"Cleaning up temp file {TempFileForRemoteCommand}");
        File.Delete(TempFileForRemoteCommand);
      }
      if ( OnExit != null ) {
        OnExit(this, EventArgs.Empty);
      }
    }

    public virtual void Stop() {
      PuttyProcess.Cancel();
    }

    protected void SetProcessTitle(string title = "") {
      if ( PuttyProcess != null ) {
        PuttyProcess.SetProcessTitle(title);
      }
    }
    #endregion --- Windows processes -------------------------------------------- 

    public static IEnumerable<Process> GetAllPuttyProcess() {
      foreach ( Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PUTTY_WITHOUT_EXTENSION) ) {
        yield return ProcessItem;
      }
      foreach ( Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PLINK_WITHOUT_EXTENSION) ) {
        yield return ProcessItem;
      }
      yield break;
    }
  }
}
