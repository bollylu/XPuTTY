using System;
using System.IO;
using System.Linq;
using BLTools;
using libxputty_std20;
using libxputty_std20.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace libxputty_tests {
  [TestClass]
  public class IPuttySessionTest {

    private readonly static string TEST_SESSION_MANAGER_FILE = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    private const int TEST_SESSION_PID = 10;
    private const string TEST_SESSION_TAG = "azerty";

    [TestMethod]
    public void CreateIPuttySession_TypeIsSSH_TypeIsOK() {
      IPuttySession Session = new TPuttySessionSSH(TSessionManager.DEFAULT_SESSION_MANAGER);
      Assert.AreEqual(TPuttyProtocol.SSH, Session.Protocol);
    }

    [TestMethod]
    public void CreateIPuttySession_TypeIsSerial_TypeIsOK() {
      IPuttySession Session = new TPuttySessionSerial(TSessionManager.DEFAULT_SESSION_MANAGER);
      Assert.AreEqual(TPuttyProtocol.Serial, Session.Protocol);
    }

    [TestMethod]
    public void CreateIPuttySession_DefaultValue_SessionManagerIsMemory() {
      IPuttySession Session = new TPuttySessionSSH(TSessionManager.DEFAULT_SESSION_MANAGER);
      Assert.IsTrue(Session.SessionManager is TSessionManagerMemory);
    }

    [TestMethod]
    public void CreateIPuttySession_SessionManagerCSV_SessionManagerFileIsCreatedAndContainsData() {
      Assert.IsFalse(File.Exists(TEST_SESSION_MANAGER_FILE), "File is not yet created");

      ISessionManager SessionManager = new TSessionManagerCSV(TEST_SESSION_MANAGER_FILE);
      IPuttySession Session = new TPuttySessionSSH(SessionManager);

      Assert.IsTrue(Session.SessionManager is TSessionManagerCSV);

      Assert.IsFalse(File.Exists(TEST_SESSION_MANAGER_FILE), "File shouldn't exist yet");

      SessionManager.AddSession(TEST_SESSION_PID, TEST_SESSION_TAG);
      Assert.IsTrue(File.Exists(TEST_SESSION_MANAGER_FILE), "File must exist now");
      Log.Write(SessionManager.GetSession(TEST_SESSION_PID).ToString());
      Log.Write(SessionManager.GetSession(TEST_SESSION_TAG).ToString());
      Assert.AreEqual(1, SessionManager.GetSessions().Count());

      SessionManager.Clear();
      Assert.IsFalse(File.Exists(TEST_SESSION_MANAGER_FILE), "File must be cleaned now");
    }

  }
}
