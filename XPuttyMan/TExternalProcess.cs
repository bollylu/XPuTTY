using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPutty {
  public class TExternalProcess {

    public string Name { get; set; }
    public string Parameters { get; set; }

    public TExternalProcess() { }
    public TExternalProcess(string name, string parameters) {
      Name = name;
      Parameters = parameters;
    }

    public async Task<string> ExecuteAsUser(ProcessStartInfo startInfo, TCredential credential) {
      using (Process ExternalProcess = new Process()) {
        ExternalProcess.StartInfo = new ProcessStartInfo("cmd.exe", $"/c \"{Name}\" {Parameters}");
        ExternalProcess.StartInfo.UseShellExecute = true;

        if (credential != null) {
          ExternalProcess.StartInfo.Verb = "runas";
          ExternalProcess.StartInfo.UserName = credential.UsernameWithoutDomain;
          ExternalProcess.StartInfo.Domain = credential.Domain;
          ExternalProcess.StartInfo.Password = credential.SecurePassword;
        }

        try {
          if (!ExternalProcess.Start()) {
            return "";
          }
        } catch {
          return "";
        }

        string ProcessResponse = await ExternalProcess.StandardOutput.ReadToEndAsync();
        await ExternalProcess.WaitForExitAsync();
        return ProcessResponse;
      }
    }
  }
}
