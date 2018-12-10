using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using libxputty_std20;
using libxputty_std20.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace libxputty_tests {
  [TestClass]
  public class SessionSourceTest {

    private const string SOURCE_TEST_FILENAME = @"c:\test\filename.xml";

    private const string XML_TEST_DATA_SOURCE =
      @"<?xml version=""1.0"" encoding=""utf-8""?>
        <Root xmlns = ""http://easyputty.sharenet.be"" >
          <Group>
            <Group Name=""Sharenet DMZ"" Id=""Sharenet_DMZ"">
              <Group Name = ""Network"" Id=""Network/Sharenet_DMZ"" >
                <Group Name=""Console"" Id=""Console/Network/Sharenet_DMZ"">
                  <Session Name = ""dl610 (download machine)"" Protocol=""SSH"" HostName=""dl610.dmz210.sharenet.priv"" PortNumber=""22"">
                    <Credential Username = ""testuser"" Secure=""false"" Password=""testpwd"" />
                  </Session>
                  <Session Name = ""Raspberry Pi"" Protocol=""SSH"" HostName=""10.100.210.64"" PortNumber=""22"">
                    <Credential Username = ""pi"" Secure=""false"" Password=""pipwd"" />
                  </Session>
                </Group>
                <Group Name=""Logs"" Id=""Logs/Network/Sharenet_DMZ"">
                  <Session Name = ""Syslog follow-up"" Protocol=""SSH"" HostName=""dl610.dmz210.sharenet.priv"" PortNumber=""22"">
                    <Credential Username = ""testuser"" Secure=""false"" Password=""testpwd"" />
                    <RemoteCommand>tail -f /var/log/syslog</RemoteCommand>
                  </Session>
                </Group>
              </Group>
            </Group>
          </Group>
        </Root>
      ";

    private const string GROUP_ID_TEST1 = "Network/Sharenet_DMZ";
    private const string GROUP_ID_WRONG = "Sharenet_DMZ/Network";

    [TestCategory("SessionSource")]
    [TestMethod]
    public void SessionSourceXml_CreateEmpty_InitOk() {
      TPuttySessionSource SourceTest = new TPuttySessionSourceXml();
      Assert.IsTrue(SourceTest.SourceType.Equals(ESourceType.Xml));
      Assert.IsTrue(SourceTest.DataSourceName.StartsWith(TPuttySessionSourceXml.DATASOURCE_PREFIX));
      Assert.IsTrue(string.IsNullOrEmpty(SourceTest.StorageLocation));
    }

    [TestCategory("SessionSource")]
    [TestMethod]
    public void SessionSourceXml_CreateFile_InitOk() {
      TPuttySessionSource SourceTest = new TPuttySessionSourceXml(SOURCE_TEST_FILENAME);
      Assert.AreEqual(SOURCE_TEST_FILENAME, SourceTest.StorageLocation);
    }

    [TestCategory("SessionSource")]
    [TestMethod]
    public void SessionSourceXml_ConvertGroupFromXml_MainGroupOk() {
      TPuttySessionSourceXml SourceTest = new TPuttySessionSourceXml(SOURCE_TEST_FILENAME);
      SourceTest.XmlDataCache = XDocument.Parse( XML_TEST_DATA_SOURCE).Root;
      IEnumerable<IPuttySessionsGroup> Groups = SourceTest.GetGroupsFrom("", false);
      Assert.AreEqual(1, Groups.Count());
    }

    [TestCategory("SessionSource")]
    [TestMethod]
    public void SessionSourceXml_ConvertGroupFromXml_SubGroupsOk() {
      TPuttySessionSourceXml SourceTest = new TPuttySessionSourceXml(SOURCE_TEST_FILENAME);
      SourceTest.XmlDataCache = XDocument.Parse(XML_TEST_DATA_SOURCE).Root;
      IEnumerable<IPuttySessionsGroup> Groups = SourceTest.GetGroupsFrom(GROUP_ID_TEST1, true);
      Assert.AreEqual(2, Groups.Count());
    }

    [TestCategory("SessionSource")]
    [TestMethod]
    public void SessionSourceXml_GetGroupsWongId_Zero() {
      TPuttySessionSourceXml SourceTest = new TPuttySessionSourceXml(SOURCE_TEST_FILENAME);
      SourceTest.XmlDataCache = XDocument.Parse(XML_TEST_DATA_SOURCE).Root;
      IEnumerable<IPuttySessionsGroup> Groups = SourceTest.GetGroupsFrom(GROUP_ID_WRONG, false);
      Assert.AreEqual(0, Groups.Count());
    }
  }
}
