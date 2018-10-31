using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using BLTools;

namespace libxputty_std20.Interfaces {
  public interface IPuttySession : IDisposable {
    string Name { get; set; }
    string CleanName { get; }
    int PID { get; }
    string CommandLine { get; }
    bool IsRunning { get; }
    TRunProcess PuttyProcess { get; }

    TPuttyProtocol Protocol { get; set; }

    void Start();
    void StartPlink();

    void Stop();

    void SaveToRegistry();
    IPuttySession LoadFromRegistry();

    event EventHandler OnStart;
    event EventHandler OnExit;

    XElement ToXml();
  }
}
