using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20 {
  public interface ISessionManager {

    IEnumerable<KeyValuePair<int,string>> GetSessions();
    KeyValuePair<int, string> GetSession(string tag);
    KeyValuePair<int, string> GetSession(int PID);

    bool IsSessionRunning(int PID);

    void AddSession(int PID, string tag);

    void RemoveSession(int PID);
    void RemoveSession(string tag);

    void Clear();

  }
}
