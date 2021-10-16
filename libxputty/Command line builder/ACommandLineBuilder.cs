using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libxputty {
  public abstract class ACommandLineBuilder : ICommandLineBuilder {

    private List<string> Args = new();
    private readonly object _Lock = new();

    public ICommandLineBuilder AddArgument(string argument) {
      lock (_Lock) {
        Args.Add(argument);
      }
      return this;
    }

    public virtual string Build() {
      lock (_Lock) {
        return string.Join(" ", Args);
      }
    }
  }
}
