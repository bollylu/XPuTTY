using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using BLTools;
using BLTools.Diagnostic.Logging;
using BLTools.Json;

namespace libxputty {
  public abstract class ASessionPutty : ASession, ISessionPutty, IDisposable, ISupportContactContainer {

    #region --- Constants --------------------------------------------
    public const string EXECUTABLE_PUTTY = "putty.exe";
    public static string EXECUTABLE_PUTTY_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PUTTY);
    public const string EXECUTABLE_PLINK = "plink.exe";
    public static string EXECUTABLE_PLINK_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PLINK);
    public const string EXECUTABLE_PSCP = "pscp.exe";
    public static string EXECUTABLE_PSCP_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PSCP);
    #endregion --- Constants --------------------------------------------

    public static ISessionPutty Empty
    {
      get
      {
        if (_Empty == null) {
          _Empty = new TSessionPuttyEmpty();
        }
        return _Empty;
      }
    }
    private static ISessionPutty _Empty;

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
      StringBuilder RetVal = new();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");
      return RetVal.ToString();
    }

    //public virtual IJsonValue ToJson() {
    //  JsonObject RetVal = new JsonObject {
    //    { JSON_THIS_ELEMENT, new JsonObject {
    //        { JSON_NAME, Json.JsonEncode(Name) },
    //        { JSON_PROTOCOL, Protocol }
    //      }
    //    }
    //  };
    //  return RetVal;
    //}
    #endregion --- Converters -------------------------------------------------------------------------------------


    #region --- Constructor(s) ---------------------------------------------------------------------------------
    protected ASessionPutty() : base() {
      Name = "<no name>";
      SessionType = ESessionPuttyType.Auto;
    }

    protected ASessionPutty(string name) : base(name) {
      SessionType = ESessionPuttyType.Auto;
    }

    protected ASessionPutty(ISessionPutty session) : base(session) {
      SessionType = session.SessionType;
      GroupLevel1 = session.GroupLevel1;
      GroupLevel2 = session.GroupLevel2;
      Section = session.Section;
      RemoteCommand = session.RemoteCommand;
      SetLocalCredential(session.Credential);
    }

    protected override void _Initialize() {
      PuttyProcess.OnExit += _PuttyProcess_OnExit;
      PuttyProcess.OnStart += _PuttyProcess_OnStart;
    }
    public override void Dispose() {
      PuttyProcess.OnExit -= _PuttyProcess_OnExit;
      PuttyProcess.OnStart -= _PuttyProcess_OnStart;
    }

    public override ISession Duplicate() {
      ISessionPutty RetVal = base.Duplicate();
      RetVal.GroupLevel1 = GroupLevel1;
      RetVal.GroupLevel2 = GroupLevel2;
      RetVal.RemoteCommand = RemoteCommand;
      RetVal.Section = Section;
      return RetVal;
    }

    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Windows processes --------------------------------------------
    public ISupportContact SupportContact => throw new NotImplementedException();

    public override void Start(string arguments = "") {

      if (!string.IsNullOrWhiteSpace(RemoteCommand)) {
        string TempFileForRemoteCommand = Path.GetTempFileName();
        Log($"Created Tempfile {TempFileForRemoteCommand}");
        File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);
      }

      switch (SessionType) {

        case ESessionType.PuttySSH:
          _StartPutty(arguments);
          break;

        case ESessionType.PLinkSSH:
          _StartPlink(arguments);
          break;

        default: {
            if (!string.IsNullOrWhiteSpace(RemoteCommand)) {
              _StartPlink(arguments);
            } else {
              _StartPutty(arguments);
            }
            break;
          }

      }

    }

    protected virtual void _StartPutty(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = arguments == "" ? $"-load {"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false
      };
      RunProcess.StartInfo = StartInfo;
      OnStart?.Invoke(this, EventArgs.Empty);

      RunProcess.Start(true);
    }
    protected virtual void _StartPlink(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = arguments == "" ? $"-load {"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false
      };
      RunProcess.StartInfo = StartInfo;

      RunProcess.Start();
    }

    private void _PuttyProcess_OnStart(object sender, EventArgs e) {
      OnStart?.Invoke(this, EventArgs.Empty);
    }

    private void _PuttyProcess_OnExit(object sender, EventArgs e) {
      if (!string.IsNullOrWhiteSpace(TempFileForRemoteCommand) && File.Exists(TempFileForRemoteCommand)) {
        Log($"Cleaning up temp file {TempFileForRemoteCommand}");
        try {
          File.Delete(TempFileForRemoteCommand);
        } catch (Exception ex) {
          LogError($"Unable to cleanup temp file {TempFileForRemoteCommand} : {ex.Message}");
        }
      }
      OnExited?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Stop() {
      OnExit?.Invoke(this, EventArgs.Empty);
      PuttyProcess.Cancel();
    }

    public static IEnumerable<Process> GetAllPuttyProcess() {
      foreach (Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PUTTY_WITHOUT_EXTENSION)) {
        yield return ProcessItem;
      }
      foreach (Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PLINK_WITHOUT_EXTENSION)) {
        yield return ProcessItem;
      }
      yield break;
    }

    #endregion --- Windows processes -------------------------------------------- 

    public static ISessionPutty BuildSessionPutty(EPuttyProtocol puttyProtocol, ILogger logger) {

      if (logger is null) {
        logger = ALogger.DEFAULT_LOGGER;
      }

      switch (puttyProtocol) {

        case EPuttyProtocol.SSH: {
            ISessionPutty RetVal = new TSessionPuttySsh();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case EPuttyProtocol.Raw: {
            ISessionPutty RetVal = new TSessionPuttyRaw();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case EPuttyProtocol.Telnet: {
            ISessionPutty RetVal = new TSessionPuttyTelnet();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case EPuttyProtocol.Serial: {
            ISessionPutty RetVal = new TSessionPuttySerial();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        default:
          return null;
      }
    }
  }


}
