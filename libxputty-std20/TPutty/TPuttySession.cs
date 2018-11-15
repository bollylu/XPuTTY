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
  public class TPuttySession : TPuttyBase, IPuttySession, IDisposable, ISupportContactContainer {

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

    public static IPuttySession Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TPuttySession("<empty>");
        }
        return _Empty;
      }
    }
    private static IPuttySession _Empty;

    #region --- Public properties ------------------------------------------------------------------------------
    public string CleanName => Name.Replace("%20", " ");
    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();

    public string RemoteCommand { get; set; }
    public string CleanedRemoteCommand => (RemoteCommand ?? "").Replace("\"", "\\\"");

    public string GroupLevel1 { get; set; }
    public string GroupLevel2 { get; set; }
    public string Section { get; set; }

    protected string SessionTitle;
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
    public TPuttySession() : base() {
      Name = "<no name>";
    }

    public TPuttySession(string name) : base(name) {
    }

    public TPuttySession(IPuttySession session) {
      Name = session.Name;
      Description = session.Description;
      Comment = session.Comment;
      GroupLevel1 = session.GroupLevel1;
      GroupLevel2 = session.GroupLevel2;
      Section = session.Section;
      RemoteCommand = session.RemoteCommand;
    }

    protected override void _Initialize() {
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;
    }
    public override void Dispose() {
      PuttyProcess.OnExit -= PuttyProcess_OnExit;
      PuttyProcess.OnStart -= PuttyProcess_OnStart;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Windows processes --------------------------------------------
    public TRunProcess PuttyProcess { get; protected set; } = new TRunProcess();
    public bool IsRunning => PuttyProcess.IsRunning;
    public int PID => PuttyProcess.PID;
    public string CommandLine => TRunProcess.GetCommandLine(PID);

    public ISupportContact SupportContact => throw new NotImplementedException();

    public virtual void Start(IEnumerable<string> arguments) {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = !arguments.Any() ? $"-load {"\"" + CleanName + "\""}" : string.Join(" ", arguments),
        UseShellExecute = false,
        RedirectStandardError = true
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }

    public virtual void Start(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = arguments == "" ? $"-load {"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false,
        RedirectStandardError = true
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }

    public virtual void StartPlink(IEnumerable<string> arguments) {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = !arguments.Any() ? $"-load {"\"" + CleanName + "\""}" : string.Join(" ", arguments),
        UseShellExecute = false,
        RedirectStandardOutput = false,
        RedirectStandardError = true
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }

    public virtual void StartPlink(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = arguments == "" ? $"-load {"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false,
        RedirectStandardOutput = false,
        RedirectStandardError = true
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
      if ( !string.IsNullOrWhiteSpace(TempFileForRemoteCommand) ) {
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
        PuttyProcess.ProcessTitle = title;
      }
    }

    public static IEnumerable<Process> GetAllPuttyProcess() {
      foreach ( Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PUTTY_WITHOUT_EXTENSION) ) {
        yield return ProcessItem;
      }
      foreach ( Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PLINK_WITHOUT_EXTENSION) ) {
        yield return ProcessItem;
      }
      yield break;
    }
    #endregion --- Windows processes -------------------------------------------- 




  }
}
