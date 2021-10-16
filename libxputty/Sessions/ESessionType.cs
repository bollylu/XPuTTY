namespace libxputty {
  public enum ESessionType {
    Unknown,      // Unidentified session
    PuttySSH,     // Putty session
    PuttySerial,  // Putty session
    PuttyRaw,     // Putty session
    PuttyTelnet,  // Putty session
    PLinkSSH,     // PLink session
    Pscp,         // Pscp session
    OpenSSH,      // OpenSSH session
    Scp,          // Scp session
    CmdPrompt,    // Command prompt (cmd.exe)
    Powershell,   // Powershell
    Custom        // Any executable
  }
}
