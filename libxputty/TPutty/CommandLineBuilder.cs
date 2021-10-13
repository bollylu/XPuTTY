using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLTools;

using libxputty.Interfaces;

namespace libxputty {
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

      if (credential == null) {
        return RetVal;
      }

      RetVal.Add($"-l {credential.Username}");
      if (!string.IsNullOrEmpty(credential.SecurePassword.ConvertToUnsecureString())) {
        RetVal.Add($"-pw {credential.SecurePassword.ConvertToUnsecureString()}");
      }
      return RetVal;
    }

    public static IList<string> AddRemoteCommandToCommandLine(this IList<string> commandLine, string tempFilename = "") {
      IList<string> RetVal = commandLine;
      if (!string.IsNullOrWhiteSpace(tempFilename)) {
        RetVal.Add($"-m \"{tempFilename}\"");
      }
      return RetVal;
    }

  }
}
