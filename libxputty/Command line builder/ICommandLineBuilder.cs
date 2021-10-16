using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libxputty {
  public interface ICommandLineBuilder {

    string Build();
    ICommandLineBuilder AddArgument(string argument);

  }
}
