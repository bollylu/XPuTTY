using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20 {
  public interface ISessionManager {

    IEnumerable<(int, string)> GetSessions();
    int GetSession(string tag);
    string GetSession(int PID);

    bool IsSessionRunning(int PID);

    void AddSession(int PID, string tag);

    void RemoveSession(int PID);
    void RemoveSession(string tag);

  }
}
