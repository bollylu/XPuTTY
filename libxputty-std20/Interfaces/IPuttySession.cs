using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
    void Stop();

    bool CheckIsRunning();
    void SetRunningProcess(Process process);

    event EventHandler OnStart;
    event EventHandler OnExit;
  }
}
