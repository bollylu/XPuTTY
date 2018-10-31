using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using BLTools;
using Microsoft.Win32;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySession : IPuttySession, IDisposable, ISupportContactContainer, IName {

    #region --- Constants --------------------------------------------
    public const string REG_KEYNAME_BASE = @"Software\SimonTatham\PuTTY\Sessions";

    public const string EXECUTABLE_PUTTY = "putty.exe";
    public static string EXECUTABLE_PUTTY_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PUTTY);
    public const string EXECUTABLE_PLINK = "plink.exe";
    public static string EXECUTABLE_PLINK_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(EXECUTABLE_PLINK);
    public const string EXECUTABLE_PSCP = "pscp.exe";

    internal const string REG_PROTOCOL_TYPE = "Protocol";

    protected const string XML_THIS_ELEMENT = "Session";
    protected const string XML_ATTRIBUTE_NAME = "Name";
    protected const string XML_ATTRIBUTE_PROTOCOL = "Protocol";
    #endregion --- Constants --------------------------------------------

    #region --- IName --------------------------------------------
    public string Name { get; set; }
    public string Description { get; set; }
    public string Comment { get; set; }
    #endregion --- IName --------------------------------------------

    #region --- Public properties ------------------------------------------------------------------------------
    public string CleanName => Name.Replace("%20", " ");
    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");
      return RetVal.ToString();
    }

    public virtual XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_PROTOCOL, Protocol);
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
      PuttyProcess = new TRunProcess();
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;

    }
    public TPuttySession(string name) {
      Name = name;
      PuttyProcess = new TRunProcess();
      PuttyProcess.OnExit += PuttyProcess_OnExit;
      PuttyProcess.OnStart += PuttyProcess_OnStart;
    }
    public virtual void Dispose() {
      PuttyProcess.OnExit -= PuttyProcess_OnExit;
      PuttyProcess.OnStart -= PuttyProcess_OnStart;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Registry interactions --------------------------------------------
    public static RegistryKey GetRegistryKey(string keyName) {
      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
      return Registry.CurrentUser.OpenSubKey(PuttySessionKeyName);
    }

    public static RegistryKey GetRegistryKeyRW(string keyName) {
      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
      return Registry.CurrentUser.CreateSubKey(PuttySessionKeyName, true);
    }

    public static TPuttyProtocol GetSessionProtocolFromRegistry(string sessionKeyName = "") {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(sessionKeyName) ) {
        return TPuttyProtocol.Unknown;
      }
      #endregion === Validate parameters ===

      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{sessionKeyName}";
      using ( RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName) ) {
        return PuttySessionKey.GetValue(REG_PROTOCOL_TYPE, "") as string;
      }
    }

    public virtual IPuttySession LoadFromRegistry() {
      return this;
    }

    public virtual void SaveToRegistry() {

    }
    #endregion --- Registry interactions --------------------------------------------

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
    public TRunProcess PuttyProcess { get; protected set; }
    public bool IsRunning => PuttyProcess.IsRunning;
    public int PID => PuttyProcess.PID;
    public string CommandLine => TRunProcess.GetCommandLine(PID);

    public ISupportContact SupportContact => throw new NotImplementedException();

    

    public virtual void Start() {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = $"-load {"\"" + CleanName + "\""}",
        UseShellExecute = false
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }

    public virtual void StartPlink() {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = $"{"\"" + CleanName + "\""}",
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
      foreach(Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PUTTY_WITHOUT_EXTENSION)) {
        yield return ProcessItem;
      }
      foreach ( Process ProcessItem in Process.GetProcessesByName(EXECUTABLE_PLINK_WITHOUT_EXTENSION) ) {
        yield return ProcessItem;
      }
      yield break;
    }
  }
}
