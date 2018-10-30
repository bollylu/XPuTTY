using BLTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace libxputty_std20.Interfaces {
  public interface ICredential : IToXml {
    string Username { get; }
    string Domain { get; }
    string UsernameWithoutDomain { get; }
    bool XmlSecure { get;}
    string EncryptionKey { set; }
    bool HasValue { get; }
    bool Inherited { get; }
    SecureString SecurePassword { get; }

    void SetSecure(bool value);
    void Dispose();
  }
}
