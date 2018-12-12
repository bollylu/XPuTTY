using System;
using libxputty_std20;
using libxputty_std20.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BLTools;
using System.Linq;

namespace libxputty_tests {

  [TestClass]
  public class IGroupItemTest {

    [TestCategory("Groups")]
    [TestMethod]
    public void CreateIGroupItem_NoParams_StructOk() {
      IPuttySessionGroup Test = new TPuttySessionGroup();
      Assert.IsFalse(Test.Groups.Any());
      Assert.IsFalse(Test.Sessions.Any());
    }

    [TestCategory("Groups")]
    [TestMethod]
    public void CreateIGroupItem_WithName_NameOk() {
      const string TestName = "Test group";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Assert.AreEqual(TestName, Test.Name);
    }

    [TestCategory("Groups")]
    [TestMethod]
    public void CreateIGroupItem_WithOneGroup_ContentOk() {
      const string TestName = "Test group";
      const string TestName2 = "Test group 2";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Test.AddOrUpdateGroup(new TPuttySessionGroup(TestName2));
      
      Assert.AreEqual(TestName, Test.Name);
      Assert.AreEqual(1, Test.Groups.Count);
      Assert.AreEqual(1, Test.GetAllGroups().Count());
    }

    [TestCategory("Groups")]
    [TestMethod]
    public void CreateIGroupItem_WithSubGroups_ContentOk() {
      const string TestName = "Test group";
      const string TestName2 = "Test group 2";
      const string TestName3 = "Test group 3";
      const string TestName4 = "Test group 4";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Test.AddOrUpdateGroup(new TPuttySessionGroup(TestName2));
      IPuttySessionGroup Test3 = new TPuttySessionGroup(TestName3);
      Test3.AddOrUpdateGroup(new TPuttySessionGroup(TestName4));
      Test.AddOrUpdateGroup(Test3);

      Assert.AreEqual(2, Test.Groups.Count);
      Assert.AreEqual(3, Test.GetAllGroups().Count());
    }

    [TestCategory("Groups")]
    [TestMethod]
    public void CreateIGroupItem_WithGroupsAddedRemoved_ContentOk() {
      const string TestName = "Test group";
      const string TestName2 = "Test group 2";
      const string TestName3 = "Test group 3";
      const string TestName4 = "Test group 4";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Test.AddOrUpdateGroup(new TPuttySessionGroup(TestName2));
      IPuttySessionGroup Test3 = new TPuttySessionGroup(TestName3);
      Test3.AddOrUpdateGroup(new TPuttySessionGroup(TestName4));
      Test.AddOrUpdateGroup(Test3);
      Test.RemoveGroup(TestName2);

      Assert.AreEqual(1, Test.Groups.Count);
      Assert.AreEqual(2, Test.GetAllGroups().Count());
    }

    [TestCategory("Groups")]
    [TestMethod]
    public void CreateIGroupItem_ClearGroups_ContentOk() {
      const string TestName = "Test group";
      const string TestName2 = "Test group 2";
      const string TestName3 = "Test group 3";
      const string TestName4 = "Test group 4";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Test.AddOrUpdateGroup(new TPuttySessionGroup(TestName2));
      IPuttySessionGroup Test3 = new TPuttySessionGroup(TestName3);
      Test3.AddOrUpdateGroup(new TPuttySessionGroup(TestName4));
      Test.AddOrUpdateGroup(Test3);
      Test.ClearGroups();

      Assert.AreEqual(0, Test.Groups.Count);
      Assert.AreEqual(0, Test.GetAllGroups().Count());
    }


    [TestCategory("Sessions")]
    [TestMethod]
    public void CreateIGroupItem_WithOneSession_ContentOk() {
      const string TestName = "Test group";
      const string TestSession1 = "Session 1";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Test.AddOrUpdateSession(new TPuttySessionSSH(TestSession1));

      Assert.AreEqual(1, Test.Sessions.Count);
      Assert.AreEqual(1, Test.GetAllSessions().Count());
    }

    [TestCategory("Sessions")]
    [TestMethod]
    public void CreateIGroupItem_WithSeveralSessions_ContentOk() {
      const string TestName = "Test group";
      const string TestSession1 = "Session 1";
      const string TestSession2 = "Session 2";
      const string TestSession3 = "Session 3";
      IPuttySessionGroup Test = new TPuttySessionGroup(TestName);
      Test.AddOrUpdateSession(new TPuttySessionSSH(TestSession1));
      Test.AddOrUpdateSession(new TPuttySessionSSH(TestSession2));
      Test.AddOrUpdateSession(new TPuttySessionSSH(TestSession3));

      Assert.AreEqual(3, Test.Sessions.Count);
      Assert.AreEqual(3, Test.GetAllSessions().Count());
    }

    [TestCategory("Sessions")]
    [TestMethod]
    public void CreateIGroupItem_WithSeveralGroupsAndSessions_ContentOk() {
      const string TestGroup1 = "Test group 1";
      const string TestGroup2 = "Test group 2";
      const string TestSession1 = "Session 1";
      const string TestSession2 = "Session 2";
      const string TestSession3 = "Session 3";
      IPuttySessionGroup Test1 = new TPuttySessionGroup(TestGroup1);
      IPuttySessionGroup Test2 = new TPuttySessionGroup(TestGroup2);
      Test1.AddOrUpdateSession(new TPuttySessionSSH(TestSession1));
      Test2.AddOrUpdateSession(new TPuttySessionSSH(TestSession2));
      Test1.AddOrUpdateSession(new TPuttySessionSSH(TestSession3));
      Test1.AddOrUpdateGroup(Test2);

      Assert.AreEqual(2, Test1.Sessions.Count);
      Assert.AreEqual(3, Test1.GetAllSessions().Count());
      Assert.AreEqual(2, Test1.GetAllSessions(false).Count());
    }

    [TestCategory("Sessions")]
    [TestMethod]
    public void CreateIGroupItem_WithGroupsAndSessionsAddedRemoved_ContentOk() {
      const string TestGroup1 = "Test group 1";
      const string TestGroup2 = "Test group 2";
      const string TestSession1 = "Session 1";
      const string TestSession2 = "Session 2";
      const string TestSession3 = "Session 3";
      IPuttySessionGroup Test1 = new TPuttySessionGroup(TestGroup1);
      IPuttySessionGroup Test2 = new TPuttySessionGroup(TestGroup2);
      Test1.AddOrUpdateSession(new TPuttySessionSSH(TestSession1));
      Test2.AddOrUpdateSession(new TPuttySessionSSH(TestSession2));
      Test1.AddOrUpdateSession(new TPuttySessionSSH(TestSession3));
      Test1.AddOrUpdateGroup(Test2);
      Test2.RemoveSession(TestSession2);

      Assert.AreEqual(2, Test1.Sessions.Count);
      Assert.AreEqual(2, Test1.GetAllSessions().Count());
    }

    [TestCategory("Sessions")]
    [TestMethod]
    public void CreateIGroupItem_ClearGroupsAndSessions_ContentOk() {
      const string TestGroup1 = "Test group 1";
      const string TestGroup2 = "Test group 2";
      const string TestSession1 = "Session 1";
      const string TestSession2 = "Session 2";
      const string TestSession3 = "Session 3";
      IPuttySessionGroup Test1 = new TPuttySessionGroup(TestGroup1);
      IPuttySessionGroup Test2 = new TPuttySessionGroup(TestGroup2);
      Test1.AddOrUpdateSession(new TPuttySessionSSH(TestSession1));
      Test2.AddOrUpdateSession(new TPuttySessionSSH(TestSession2));
      Test1.AddOrUpdateSession(new TPuttySessionSSH(TestSession3));
      Test1.AddOrUpdateGroup(Test2);

      Test2.ClearSessions();
      Assert.AreEqual(2, Test1.Sessions.Count);
      Assert.AreEqual(2, Test1.GetAllSessions().Count());
      Assert.AreEqual(1, Test1.GetAllGroups().Count());

      Test1.Clear();
      Assert.AreEqual(0, Test1.Sessions.Count);
      Assert.AreEqual(0, Test1.Groups.Count);
      Assert.AreEqual(0, Test1.GetAllSessions().Count());
    }
  }
}
