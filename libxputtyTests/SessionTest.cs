using Microsoft.VisualStudio.TestTools.UnitTesting;
using libxputty;

namespace libxputtyTests {
  [TestClass]
  public class SessionTest {

    public const string SESSION_NAME = "SessionTest";
    public const string SESSION_DESCRIPTION = "NiceSession for test";
    public const string SESSION_COMMENT = "This is comment";
    public const string SESSION_STORAGE_LOCATION = @"c:\sessions";
    public const string SESSION_SSH_HOSTNAME = @"server.sharenet.priv";
    public const int SESSION_SSH_PORT = 1234;
    public const ESessionPuttyType SESSION_TYPE = ESessionPuttyType.Putty;

    [TestMethod]
    public void CreateSession() {
      ISession Target = new TSessionPuttyEmpty() {
        Name = SESSION_NAME,
        Comment = SESSION_COMMENT,
        Description = SESSION_DESCRIPTION,
        StorageLocation = SESSION_STORAGE_LOCATION,
        SessionType = ESessionPuttyType.Putty
      };

      Assert.IsNotNull(Target);
      Assert.IsTrue(Target is ISessionPutty);
      Assert.AreEqual(SESSION_NAME, Target.Name);
      Assert.AreEqual(SESSION_DESCRIPTION, Target.Description);
      Assert.AreEqual(SESSION_COMMENT, Target.Comment);
      Assert.AreEqual(SESSION_STORAGE_LOCATION, Target.StorageLocation);

      ISessionPutty PuttySession = Target as ISessionPutty;
      Assert.AreEqual(SESSION_TYPE, PuttySession.SessionType);
    }

    [TestMethod]
    public void DuplicateSessionEmpty() {
      ISession Source = new TSessionPuttyEmpty() {
        Name = SESSION_NAME,
        Comment = SESSION_COMMENT,
        Description = SESSION_DESCRIPTION,
        StorageLocation = SESSION_STORAGE_LOCATION
      };

      ISession Target = Source.Duplicate();
      Assert.IsNotNull(Target);
      Assert.IsTrue(Target is ISessionPutty);
      Assert.AreEqual(SESSION_NAME, Target.Name);
      Assert.AreEqual(SESSION_DESCRIPTION, Target.Description);
      Assert.AreEqual(SESSION_COMMENT, Target.Comment);
      Assert.AreEqual(SESSION_STORAGE_LOCATION, Target.StorageLocation);
    }

    [TestMethod]
    public void DuplicateSessionSsh() {
      ISession Source = new TSessionPuttySsh() {
        Name = SESSION_NAME,
        Comment = SESSION_COMMENT,
        Description = SESSION_DESCRIPTION,
        StorageLocation = SESSION_STORAGE_LOCATION,
        HostName = SESSION_SSH_HOSTNAME,
        Port = SESSION_SSH_PORT
      };

      ISession Target = Source.Duplicate();
      Assert.IsNotNull(Target);
      Assert.IsTrue(Target is TSessionPuttySsh);
      Assert.AreEqual(SESSION_NAME, Target.Name);
      Assert.AreEqual(SESSION_DESCRIPTION, Target.Description);
      Assert.AreEqual(SESSION_COMMENT, Target.Comment);
      Assert.AreEqual(SESSION_STORAGE_LOCATION, Target.StorageLocation);
      Assert.AreEqual(SESSION_SSH_HOSTNAME, ((TSessionPuttySsh)Target).HostName);
      Assert.AreEqual(SESSION_SSH_PORT, ((TSessionPuttySsh)Target).Port);
    }

    [TestMethod]
    public void DuplicateSessionHAP() {
      ISession Source = new TSessionPuttySsh() {
        Name = SESSION_NAME,
        Comment = SESSION_COMMENT,
        Description = SESSION_DESCRIPTION,
        StorageLocation = SESSION_STORAGE_LOCATION,
        HostName = SESSION_SSH_HOSTNAME,
        Port = SESSION_SSH_PORT
      };

      ISession Target = Source.Duplicate();
      Assert.IsNotNull(Target);
      Assert.IsTrue(Target is TSessionPuttySsh);
      Assert.AreEqual(SESSION_NAME, Target.Name);
      Assert.AreEqual(SESSION_DESCRIPTION, Target.Description);
      Assert.AreEqual(SESSION_COMMENT, Target.Comment);
      Assert.AreEqual(SESSION_STORAGE_LOCATION, Target.StorageLocation);
      Assert.AreEqual(SESSION_SSH_HOSTNAME, ((IHostAndPort)Target).HostName);
      Assert.AreEqual(SESSION_SSH_PORT, ((IHostAndPort)Target).Port);
    }

    [TestMethod]
    public void EnsureProcessIsNotRunning() {
      ISession Target = new TSessionPuttyEmpty() {
        Name = SESSION_NAME,
        Comment = SESSION_COMMENT,
        Description = SESSION_DESCRIPTION,
        StorageLocation = SESSION_STORAGE_LOCATION
      };

      Assert.IsFalse(Target.IsRunning);
    }

  }
}
