using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20 {
  public interface IPuttySession : IDisposable {
    string Name { get; set; }
    string CleanName { get; }
    int PID { get; }
    TPuttyProtocol Protocol { get; set; }

    void Start();
    void Stop();

    bool CheckIsRunning();

    event EventHandler OnStart;
    event EventHandler OnExit;
  }
}
