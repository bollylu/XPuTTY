using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BLTools;

namespace XPuttyMan {
  public static class Helpers {
    public static SplitArgs ReadAppArgs() {
      if ( ApplicationDeployment.IsNetworkDeployed ) {
        if ( AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData == null ) {
          return new SplitArgs(new string[] { $"\"{Assembly.GetEntryAssembly().FullName}\"" });
        } else {
          return new SplitArgs(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0]);
        }

      } else {
        return new SplitArgs(Environment.GetCommandLineArgs());
      }

    }

  }
}
