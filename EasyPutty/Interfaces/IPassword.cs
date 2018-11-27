using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EasyPutty.Interfaces {
  public interface IPassword {

    SecureString GetPassword();
    void SetPassword(SecureString password);

  }
}
