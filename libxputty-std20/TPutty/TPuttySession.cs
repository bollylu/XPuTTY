using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using BLTools;
using BLTools.Encryption;
using BLTools.Json;

using libxputty_std20.Interfaces;
using static libxputty_std20.LocalExtensions;

namespace libxputty_std20 {
  public class TPuttySession : TPuttyBase, IPuttySession, IDisposable, ISupportContactContainer {

    #region --- Constants --------------------------------------------
    public const string EXECUTABLE_PUTTY = "putty.exe";
    public static string EXECUTABLE_PUTTY_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PUTTY);
    public const string EXECUTABLE_PLINK = "plink.exe";
    public static string EXECUTABLE_PLINK_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PLINK);
    public const string EXECUTABLE_PSCP = "pscp.exe";
    public static string EXECUTABLE_PSCP_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PSCP);
    #endregion --- Constants --------------------------------------------

    public static IPuttySession Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TPuttySession(EMPTY);
        }
        return _Empty;
      }
    }
    private static IPuttySession _Empty;

    #region --- Public properties ------------------------------------------------------------------------------
    public string ID {
      get {
        if (string.IsNullOrEmpty(_ID)) {
          StringBuilder IdMaker = new StringBuilder(Name);
          IPuttySessionsGroup Owner = GetParent<IPuttySessionsGroup>();
          while (Owner!=null) {
            IdMaker.Append(Owner.Name);
            Owner = Owner.GetParent<IPuttySessionsGroup>();
          }
          _ID = IdMaker.ToString().HashToBase64(THashingMethods.MD5);
        }
        return _ID;
      }
      set {
        _ID = value;
      }
    }
    private string _ID;

    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();

    public ESessionType SessionType { get; set; }

    public string RemoteCommand { get; set; }
    public string CleanedRemoteCommand => (RemoteCommand ?? "").Replace("\"", "\\\"");

    public string GroupLevel1 { get; set; }
    public string GroupLevel2 { get; set; }
    public string Section { get; set; }

    protected string SessionTitle;

    public IList<ISupportContact> SupportContacts { get; } = new List<ISupportContact>();

    #endregion --- Public properties ---------------------------------------------------------------------------

    protected string TempFileForRemoteCommand;

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {Name.PadRight(80, '.')} : ");
      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Event handlers --------------------------------------------
    public event EventHandler OnStart;
    public event EventHandler OnExit;
    #endregion --- Event handlers --------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySession() : base() {
      Name = "<no name>";
      SessionType = ESessionType.Auto;
    }

    public TPuttySession(string name) : base(name) {
      SessionType = ESessionType.Auto;
    }

    public TPuttySession(IPuttySession session) {
      SessionType = session.SessionType;
      Name = session.Name;
      Description = session.Description;
      Comment = session.Comment;
      GroupLevel1 = session.GroupLevel1;
      GroupLevel2 = session.GroupLevel2;
      Section = session.Section;
      RemoteCommand = session.RemoteCommand;
      Parent = session.Parent;
      SetLocalCredential(session.Credential);
    }

    protected override void _Initialize() {
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;
    }
    public override void Dispose() {
      PuttyProcess.OnExit -= PuttyProcess_OnExit;
      PuttyProcess.OnStart -= PuttyProcess_OnStart;
    }

    public virtual IPuttySession Duplicate() {
      return new TPuttySession(this);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Windows processes --------------------------------------------
    public TRunProcess PuttyProcess { get; protected set; } = new TRunProcess();
    public bool IsRunning => PuttyProcess.IsRunning;
    public int PID => PuttyProcess.PID;
    public string CommandLine => TRunProcess.GetCommandLine(PID);

    public virtual void Start(IEnumerable<string> arguments) {
      Start(string.Join(" ", arguments));
    }
    public virtual void Start(string arguments = "") {

      if ( RemoteCommand != "" ) {
        string TempFileForRemoteCommand = Path.GetTempFileName();
        Log.Write($"Created Tempfile {TempFileForRemoteCommand}");
        File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);
      }

      switch ( SessionType ) {

        case ESessionType.Putty:
          _StartPutty(arguments);
          break;

        case ESessionType.Plink:
          _StartPlink(arguments);
          break;

        case ESessionType.Auto:
        default: {
            if ( RemoteCommand != "" ) {
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
        Arguments = arguments == "" ? $"-load {"\"" + Name + "\""}" : arguments,
        UseShellExecute = false
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start(true);
    }
    protected virtual void _StartPlink(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = arguments == "" ? $"-load {"\"" + Name + "\""}" : arguments,
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
      if ( !string.IsNullOrWhiteSpace(TempFileForRemoteCommand) && File.Exists(TempFileForRemoteCommand) ) {
        Log.Write($"Cleaning up temp file {TempFileForRemoteCommand}");
        try {
          File.Delete(TempFileForRemoteCommand);
        } catch ( Exception ex ) {
          Log.Write($"Unable to cleanup temp file {TempFileForRemoteCommand} : {ex.Message}");
        }
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
  public static class CommandLineBuilder {
    public static IList<string> BuildSSHCommandLine() {
      IList<string> RetVal = new List<string>() {
        "-t",
        "-ssh"
        };
      return RetVal;
    }

    public static IList<string> AddHostnameAndPort(this IList<string> commandLine, string hostname, int port) {
      IList<string> RetVal = commandLine;
      RetVal.Add($"-P {port}");
      RetVal.Add(hostname);
      return RetVal;
    }
    public static IList<string> AddCredentialsToCommandLine(this IList<string> commandLine, ICredential credential) {
      IList<string> RetVal = commandLine;

      if ( credential == null ) {
        return RetVal;
      }

      RetVal.Add($"-l {credential.Username}");
      if ( !string.IsNullOrEmpty(credential.SecurePassword.ConvertToUnsecureString()) ) {
        RetVal.Add($"-pw {credential.SecurePassword.ConvertToUnsecureString()}");
      }
      return RetVal;
    }

    public static IList<string> AddRemoteCommandToCommandLine(this IList<string> commandLine, string tempFilename = "") {
      IList<string> RetVal = commandLine;
      if ( !string.IsNullOrWhiteSpace(tempFilename) ) {
        RetVal.Add($"-m \"{tempFilename}\"");
      }
      return RetVal;
    }

  }
}
