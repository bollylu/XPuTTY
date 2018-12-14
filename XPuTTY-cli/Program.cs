﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BLTools;
using libxputty_std20;
using libxputty_std20.Interfaces;
using static BLTools.ConsoleExtension.ConsoleExtension;

namespace XPuTTY_cli {
  internal class Program {

    #region --- Command line arguments definition --------------------------------------------
    private const string ARG_COMMAND = "cmd";
    private const string ARG_HELP = "help";
    private const string ARG_HELP2 = "h";
    private const string ARG_HELP3 = "?";
    private const string ARG_SOURCE = "source";

    private const string CMD_LIST = "list";
    private const string CMD_START = "start";
    private const string CMD_START_MENU = "menu";
    #endregion --- Command line arguments definition --------------------------------------------

    private static ISessionManager SessionManager = new TSessionManagerCSV(Path.Combine(Path.GetTempPath(), @"xputty_cli.tmp"));

    private static void Main(string[] args) {
      SplitArgs Args = new SplitArgs(args);

      if ( Args.IsDefined(ARG_HELP) || Args.IsDefined(ARG_HELP2) || Args.IsDefined(ARG_HELP3) ) {
        Usage();
      }

      string Command = Args.GetValue<string>(ARG_COMMAND, CMD_LIST).ToLower();
      string Source = Args.GetValue<string>(ARG_SOURCE, CMD_START_MENU);

      bool NeedRunning = true;
      do {

        IPuttySessionSource SessionSource = TPuttySessionSource.GetPuttySessionSource(Source, SessionManager);
        if ( SessionSource == null ) {
          Usage($"Invalid session source : {Source}");
        }

        IEnumerable<IPuttySession> Sessions = SessionSource.GetSessions();

        switch ( Command ) {
          case CMD_LIST:
            Console.WriteLine(Sessions.ToString());
            NeedRunning = false;
            break;

          case CMD_START:
            if ( Source.ToLower().StartsWith(CMD_START_MENU) ) {
              IEnumerable<IPuttySession> Menu = (new List<IPuttySession>() { new TPuttySessionSSH(SessionManager) { Name = "Cancel" } }).Concat(Sessions.Where(x => x.Protocol.IsSSH));
              Console.Clear();
              int Choice = InputList(Menu.Select(x => x.Name), "Sessions list", "Please select session to start : ", "Please select only numbers available in the list");
              if ( Choice == 1 ) {
                NeedRunning = false;
                break;
              }
              IPuttySession Selected = Menu.ElementAt(Choice);
              Console.WriteLine($"You have select session {Selected.Name}");
              Selected.Start();
              break;
            }
            if ( SessionSource.SourceType == ESourceType.Registry ) {
              break;
            }
            Usage("Invalid command");
            break;

          default:
            Usage("Missing or invalid command");
            break;
        }
      } while ( NeedRunning );

      Pause();
      Environment.Exit(0);
    }

    private static void Usage(string message = "") {
      if ( message != "" ) {
        Console.WriteLine(message);
      }

      Console.WriteLine("XPuTTY-cli v0.2 (c) 2018 Luc Bolly");
      Console.WriteLine("Usage: xputty-cli [-cmd=list|add|delete|edit|export|import|start] (default=list)");
      Console.WriteLine("                  [-source=reg:<reg entry>|menu)");

      Environment.Exit(1);
    }
  }
}
