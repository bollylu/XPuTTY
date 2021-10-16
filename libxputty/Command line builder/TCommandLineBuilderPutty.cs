using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLTools;

namespace libxputty {
  public class TCommandLineBuilderPutty : ACommandLineBuilder {

    public TCommandLineBuilderPutty() {
      AddArgument("-t");
      AddArgument("-ssh");
    }

    public TCommandLineBuilderPutty AddHostnameAndPort(string hostname, int port) {
      AddArgument($"-P {port}");
      AddArgument(hostname);
      return this;
    }

    public TCommandLineBuilderPutty AddCredentials(ICredential credential) {

      if (credential is null) {
        return this;
      }

      AddArgument($"-l {credential.Username}");
      if (!string.IsNullOrEmpty(credential.SecurePassword.ConvertToUnsecureString())) {
        AddArgument($"-pw {credential.SecurePassword.ConvertToUnsecureString()}");
      }
      return this;
    }

    public TCommandLineBuilderPutty AddRemoteCommand(string tempFilename = "") {
      if (!string.IsNullOrWhiteSpace(tempFilename)) {
        AddArgument($"-m \"{tempFilename}\"");
      }
      return this;
    }

  }
}
