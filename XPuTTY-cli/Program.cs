using System;
using System.Collections.Generic;
using System.Linq;
using BLTools;
using static BLTools.ConsoleExtension.ConsoleExtension;
using libxputty;
using libxputty.Interfaces;

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
    private const string CMD_START_REG = "reg:";
    #endregion --- Command line arguments definition --------------------------------------------

    private static void Main(string[] args) {
      SplitArgs Args = new SplitArgs(args);

      if ( Args.IsDefined(ARG_HELP) || Args.IsDefined(ARG_HELP2) || Args.IsDefined(ARG_HELP3) ) {
        Usage();
      }

      string Command = Args.GetValue<string>(ARG_COMMAND, CMD_LIST).ToLower();
      string Source = Args.GetValue<string>(ARG_SOURCE, CMD_START_MENU);

      bool NeedRunning = true;
      do {

        TPuttySessionSourceRegistry RegSource = new TPuttySessionSourceRegistry();

        IEnumerable<IPuttySession> Sessions = RegSource.GetSessions();

        switch ( Command ) {
          case CMD_LIST:
            //Console.WriteLine(Sessions.SaveToJson());
            NeedRunning = false;
            break;

          case CMD_START:
            if ( Source.ToLower().StartsWith(CMD_START_MENU) ) {
              IEnumerable<IPuttySession> Menu = (new List<IPuttySession>() { new TPuttySessionSSH() { Name = "Cancel" } }).Concat(Sessions.Where(x => x.Protocol.IsSSH));
              Console.Clear();
              int Choice = InputList(Menu.Select(x => x.CleanName), "Sessions list", "Please select session to start : ", "Please select only numbers available in the list");
              if ( Choice == 1 ) {
                NeedRunning = false;
                break;
              }
              IPuttySession Selected = Menu.ElementAt(Choice);
              Console.WriteLine($"You have select session {Selected.CleanName}");
              Selected.Start();
              break;
            }
            if ( Source.ToLower().StartsWith(CMD_START_REG) ) {
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

      Console.WriteLine("XPuTTY-cli v0.1 (c) 2018 Luc Bolly");
      Console.WriteLine("Usage: xputty-cli [-cmd=list|add|delete|edit|export|import|start] (default=list)");
      Console.WriteLine("                  [-source=reg:<reg entry>|menu)");

      Environment.Exit(1);
    }
  }
}
