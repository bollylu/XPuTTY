﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using BLTools;
using BLTools.Json;

namespace libxputty_std20.Interfaces {
  public interface IPuttySession : IDisposable, IName, ICredentialContainer {

    string GroupLevel1 { get; set; }
    string GroupLevel2 { get; set; }
    string Section { get; set; }
    string RemoteCommand { get; set; }

    string CleanName { get; }
    int PID { get; }
    string CommandLine { get; }
    bool IsRunning { get; }
    TRunProcess PuttyProcess { get; }

    TPuttyProtocol Protocol { get; set; }

    IParent Parent { get; }

    void Start(string arguments = "");
    void StartPlink(string arguments = "");

    void Stop();

    IEnumerable<string> BuildCommandLine();
    IEnumerable<string> BuildCommandLineWithoutRemoteCommand();

    event EventHandler OnStart;
    event EventHandler OnExit;

    IJsonValue ToJson();
  }
}
