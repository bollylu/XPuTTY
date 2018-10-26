using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace libxputty_std20 {
  public interface IPuttySession : IDisposable {
    string Name { get; set; }
    string CleanName { get; }
    int PID { get; }
    string CommandLine { get; }
    bool IsRunning { get; }
    Process PuttyProcess { get; }

    TPuttyProtocol Protocol { get; set; }

    void Start();
    void StartPlink();

    void Stop();

    bool CheckIsRunning();
    void SetRunningProcess(Process process);
    void SaveToRegistry();
    IPuttySession LoadFromRegistry();

    event EventHandler OnStart;
    event EventHandler OnExit;

    XElement ToXml();
  }
}
