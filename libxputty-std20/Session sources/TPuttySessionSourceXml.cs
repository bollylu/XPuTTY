using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BLTools;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionSourceXml : TPuttySessionSource {

    #region --- Constants --------------------------------------------
    protected const string XML_ELEMENT_SESSIONS = "Sessions";

    protected const string XML_ELEMENT_SESSION = "Session";
    protected const string XML_ATTRIBUTE_NAME = "Name";
    protected const string XML_ATTRIBUTE_GROUP_LEVEL1 = "GroupLevel1";
    protected const string XML_ATTRIBUTE_GROUP_LEVEL2 = "GroupLevel2";
    protected const string XML_ATTRIBUTE_SECTION = "Section";
    protected const string XML_ATTRIBUTE_PROTOCOL_TYPE = "Protocol";

    protected const string XML_ELEMENT_SESSION_TELNET = "SessionTelnet";
    protected const string XML_ELEMENT_SESSION_RLOGIN = "SessionRLogin";
    protected const string XML_ELEMENT_SESSION_RAW = "SessionRaw";

    protected const string XML_ATTRIBUTE_HOSTNAME = "HostName";
    protected const string XML_ATTRIBUTE_PORT = "PortNumber";
    protected const string XML_ELEMENT_SSH_REMOTE_COMMAND = "RemoteCommand";

    protected const string XML_ATTRIBUTE_SERIAL_LINE = "SerialLine";
    protected const string XML_ATTRIBUTE_SERIAL_SPEED = "SerialSpeed";
    protected const string XML_ATTRIBUTE_SERIAL_DATA_BITS = "SerialDataBits";
    protected const string XML_ATTRIBUTE_SERIAL_PARITY = "SerialParity";
    protected const string XML_ATTRIBUTE_SERIAL_STOP_BITS = "SerialStopHalfBits";
    protected const string XML_ATTRIBUTE_SERIAL_FLOW_CONTROL = "SerialFlowControl";
    #endregion --- Constants --------------------------------------------

    public override string DataSourceName => $@"xml://{Location ?? ""}";

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSourceXml() : base() {
      SourceType = ESourceType.Xml;
    }
    public TPuttySessionSourceXml(string location) : base() {
      SourceType = ESourceType.Xml;
      Location = location;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    protected IPuttySession _ConvertFromXml(XElement session) {

      string Name = session.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");
      TPuttyProtocol Protocol = session.SafeReadAttribute<string>(XML_ATTRIBUTE_PROTOCOL_TYPE, EPuttyProtocol.Unknown.ToString());


      if ( string.IsNullOrWhiteSpace(Name) ) {
        Log.Write($"Error while converting XML Putty session : Name is empty or invalid");
        return TPuttySession.Empty;
      }

      switch ( Protocol.Value ) {
        case EPuttyProtocol.SSH: {
            TPuttySessionSSH NewSession = new TPuttySessionSSH(Name) {
              GroupLevel1 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL1, ""),
              GroupLevel2 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL2, ""),
              Section = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SECTION, ""),
              HostName = session.SafeReadAttribute<string>(XML_ATTRIBUTE_HOSTNAME, ""),
              Port = session.SafeReadAttribute<int>(XML_ATTRIBUTE_PORT, 0),
              RemoteCommand = session.SafeReadElementValue<string>(XML_ELEMENT_SSH_REMOTE_COMMAND)
            };
            return NewSession;
          }

        case EPuttyProtocol.Serial: {
            TPuttySessionSerial NewSession = new TPuttySessionSerial(Name) {
              GroupLevel1 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL1, ""),
              GroupLevel2 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL2, ""),
              Section = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SECTION, ""),
              SerialLine = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SERIAL_LINE, ""),
              SerialSpeed = session.SafeReadAttribute<int>(XML_ATTRIBUTE_SERIAL_SPEED, 9600),
              SerialDataBits = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_DATA_BITS, 0),
              SerialStopBits = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_STOP_BITS, 0),
              SerialParity = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_PARITY, 0)
            };
            return NewSession;
          }

        case EPuttyProtocol.Telnet: {
            TPuttySessionTelnet NewSession = new TPuttySessionTelnet(Name) {
              GroupLevel1 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL1, ""),
              GroupLevel2 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL2, ""),
              Section = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SECTION, ""),
              HostName = session.SafeReadAttribute<string>(XML_ATTRIBUTE_HOSTNAME, ""),
              Port = session.SafeReadAttribute<int>(XML_ATTRIBUTE_PORT, 0),
            };
            return NewSession;
          }

        case EPuttyProtocol.RLogin: {
            TPuttySessionRLogin NewSession = new TPuttySessionRLogin(Name) {
              GroupLevel1 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL1, ""),
              GroupLevel2 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL2, ""),
              Section = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SECTION, ""),
              HostName = session.SafeReadAttribute<string>(XML_ATTRIBUTE_HOSTNAME, ""),
              Port = session.SafeReadAttribute<int>(XML_ATTRIBUTE_PORT, 0),
            };
            return NewSession;
          }

        case EPuttyProtocol.Raw: {
            TPuttySessionRaw NewSession = new TPuttySessionRaw(Name) {
              GroupLevel1 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL1, ""),
              GroupLevel2 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL2, ""),
              Section = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SECTION, ""),
              HostName = session.SafeReadAttribute<string>(XML_ATTRIBUTE_HOSTNAME, ""),
              Port = session.SafeReadAttribute<int>(XML_ATTRIBUTE_PORT, 0),
            };
            return NewSession;
          }

        default:
          Log.Write("Unable to convert session from XML : Protocol type is invalid");
          return TPuttySession.Empty;
      }
    }

    protected XElement _ConvertToXml(IPuttySession session) {

      XElement RetVal = new XElement(XML_ELEMENT_SESSION);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, session.Name);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_PROTOCOL_TYPE, session.Protocol);
      if ( !string.IsNullOrWhiteSpace(session.GroupLevel1) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_GROUP_LEVEL1, session.GroupLevel1);
      }
      if ( !string.IsNullOrWhiteSpace(session.GroupLevel2) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_GROUP_LEVEL2, session.GroupLevel2);
      }
      if ( !string.IsNullOrWhiteSpace(session.Section) ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SECTION, session.Section);
      }

      switch ( session ) {
        case TPuttySessionSSH PuttySession:
          RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, PuttySession.HostName);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, PuttySession.Port);
          if ( !string.IsNullOrWhiteSpace(PuttySession.RemoteCommand) ) {
            RetVal.SetElementValue(XML_ELEMENT_SSH_REMOTE_COMMAND, PuttySession.RemoteCommand);
          }
          break;

        case TPuttySessionSerial PuttySession:
          RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_LINE, PuttySession.SerialLine);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_SPEED, PuttySession.SerialSpeed);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_DATA_BITS, PuttySession.SerialDataBits);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_STOP_BITS, PuttySession.SerialStopBits);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_PARITY, PuttySession.SerialParity);
          break;

        case TPuttySessionTelnet PuttySession:
          RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, PuttySession.HostName);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, PuttySession.Port);
          break;

        case TPuttySessionRLogin PuttySession:
          RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, PuttySession.HostName);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, PuttySession.Port);
          break;

        case TPuttySessionRaw PuttySession:
          RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, PuttySession.HostName);
          RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, PuttySession.Port);
          break;

        default:
          Log.Write("Unable to convert session from XML : Protocol type is invalid");
          RetVal = null;
          break;
      }

      return RetVal;
    }

    protected XElement _ConvertToXml(IEnumerable<IPuttySession> sessions) {
      XElement RetVal = new XElement(XML_ELEMENT_SESSIONS);
      foreach ( IPuttySession PuttySessionItem in sessions ) {
        RetVal.Add(_ConvertToXml(PuttySessionItem));
      }
      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    protected override IEnumerable<(string, TPuttyProtocol)> _GetSessionList() {
      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        yield break;
      }

      XElement Root;
      try {
        Root = XDocument.Load(Location).Root;
        if ( !Root.Elements(XML_ELEMENT_SESSIONS).Any() ) {
          yield break;
        }
      } catch ( Exception ex ) {
        Log.Write($"Unable to read data from XML file {Location} : {ex.Message}");
        yield break;
      }

      foreach ( XElement SessionItem in Root.Elements(XML_ELEMENT_SESSIONS) ) {
        string SessionName = SessionItem.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");
        TPuttyProtocol SessionProtocol = SessionItem.SafeReadAttribute<string>(XML_ATTRIBUTE_PROTOCOL_TYPE, "Unknown");
        yield return (SessionName, SessionProtocol);
      }
    }

    protected override IPuttySession _ReadSession(string name, TPuttyProtocol protocol) {

      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        return TPuttySession.Empty;
      }

      XElement Root;
      try {
        Root = XDocument.Load(Location).Root;
        if ( !Root.Elements(XML_ELEMENT_SESSIONS).Any() ) {
          return TPuttySession.Empty;
        }
      } catch ( Exception ex ) {
        Log.Write($"Unable to read data from XML file {Location} : {ex.Message}");
        return TPuttySession.Empty;
      }

      IEnumerable<XElement> PuttySessions = Root.Elements(XML_ELEMENT_SESSIONS);
      XElement RequestedSession = PuttySessions.FirstOrDefault(x => x.Attribute(XML_ATTRIBUTE_NAME).Value == name && x.Attribute(XML_ATTRIBUTE_PROTOCOL_TYPE).Value == protocol.ToString());

      if ( RequestedSession == null ) {
        return TPuttySession.Empty;
      }

      return _ConvertFromXml(RequestedSession);

    }

    protected override IEnumerable<IPuttySession> _ReadSessions() {
      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        yield break;
      }

      XElement Root;
      try {
        Root = XDocument.Load(Location).Root;
        if ( !Root.Elements(XML_ELEMENT_SESSIONS).Any() ) {
          yield break;
        }
      } catch ( Exception ex ) {
        Log.Write($"Unable to read data from XML file {Location} : {ex.Message}");
        yield break;
      }

      IEnumerable<XElement> PuttySessions = Root.Elements(XML_ELEMENT_SESSIONS);
      foreach ( XElement SessionItem in PuttySessions ) {
        yield return _ConvertFromXml(SessionItem);
      }
    }

    protected override void _SaveSession(IPuttySession session) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      XDocument ExportFile = new XDocument(new XDeclaration("1.0", Encoding.UTF8.EncodingName, "yes"));
      XElement Root = new XElement("Root");
      XElement Sessions = new XElement(XML_ELEMENT_SESSIONS);
      Sessions.Add(_ConvertToXml(session));
      Root.Add(Sessions);
      ExportFile.Add(Root);
      try {
        ExportFile.Save(Location);
      } catch ( Exception ex ) {
        Log.Write($"Unable to export sessions in XML file {Location} : {ex.Message}");
      }

    }

    protected override void _SaveSessions(IEnumerable<IPuttySession> sessions) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      XDocument ExportFile = new XDocument(new XDeclaration("1.0", Encoding.UTF8.EncodingName, "yes"));
      XElement Root = new XElement("Root");
      Root.Add(_ConvertToXml(sessions));
      ExportFile.Add(Root);
      try {
        ExportFile.Save(Location);
      } catch ( Exception ex ) {
        Log.Write($"Unable to export sessions in XML file {Location} : {ex.Message}");
      }
    }

  }
}
