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

    public const string DATASOURCE_PREFIX = "xml";
    #endregion --- Constants --------------------------------------------

    public override string DataSourceName => $@"{DATASOURCE_PREFIX}://{Location ?? ""}";

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

      TPuttySession BaseSession = new TPuttySession(Name) {
        GroupLevel1 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL1, "<empty>"),
        GroupLevel2 = session.SafeReadAttribute<string>(XML_ATTRIBUTE_GROUP_LEVEL2, "<empty>"),
        Section = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SECTION, "<empty>"),
        RemoteCommand = session.SafeReadElementValue<string>(XML_ELEMENT_SSH_REMOTE_COMMAND, "")
      };

      if (Protocol.IsSerial) {
        TPuttySessionSerial NewSession = new TPuttySessionSerial(BaseSession) {
          SerialLine = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SERIAL_LINE, ""),
          SerialSpeed = session.SafeReadAttribute<int>(XML_ATTRIBUTE_SERIAL_SPEED, 9600),
          SerialDataBits = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_DATA_BITS, 0),
          SerialStopBits = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_STOP_BITS, 0),
          SerialParity = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_PARITY, 0),
          SerialFlowControl = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SERIAL_FLOW_CONTROL, "")
        };
        return NewSession;
      }

      IHostAndPort SessionHAP;

      switch ( Protocol.Value ) {
        case EPuttyProtocol.SSH: 
            SessionHAP = new TPuttySessionSSH(BaseSession);
            break;

        case EPuttyProtocol.Telnet: 
            SessionHAP = new TPuttySessionTelnet(BaseSession);
            break;

        case EPuttyProtocol.RLogin: 
            SessionHAP = new TPuttySessionRLogin(BaseSession);
            break;

        case EPuttyProtocol.Raw: 
            SessionHAP = new TPuttySessionRaw(BaseSession);
            break;

        default:
          Log.Write("Unable to convert session from XML : Protocol type is missing or invalid");
          return TPuttySession.Empty;
      }

      SessionHAP.HostName = session.SafeReadAttribute<string>(XML_ATTRIBUTE_HOSTNAME, "");
      SessionHAP.Port = session.SafeReadAttribute<int>(XML_ATTRIBUTE_PORT, 0);

      return SessionHAP as IPuttySession;
    }

    protected IEnumerable<IPuttySession> _ConvertFromXml(IEnumerable<XElement> sessions) {
      if ( sessions == null || !sessions.Any() ) {
        yield break;
      }
      foreach ( XElement SessionItem in sessions ) {
        yield return _ConvertFromXml(SessionItem);
      }
      yield break;
    }

    protected XElement _ConvertToXml(IPuttySession session) {

      XElement RetVal = new XElement(XML_ELEMENT_SESSION);

      string StrippedName = session.CleanName.Replace($"[{session.GroupLevel1}]", "")
                                             .Replace($"[{session.GroupLevel2}]", "")
                                             .Replace($"{{{session.Section}}}", "");
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, StrippedName);

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

      if ( session is IHostAndPort SessionHAP ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, SessionHAP.HostName);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, SessionHAP.Port);
      }

      if ( !string.IsNullOrWhiteSpace(session.RemoteCommand) ) {
        RetVal.SetElementValue(XML_ELEMENT_SSH_REMOTE_COMMAND, session.RemoteCommand);
      }

      if ( session is TPuttySessionSerial PuttySession ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_LINE, PuttySession.SerialLine);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_SPEED, PuttySession.SerialSpeed);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_DATA_BITS, PuttySession.SerialDataBits);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_STOP_BITS, PuttySession.SerialStopBits);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_PARITY, PuttySession.SerialParity);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_FLOW_CONTROL, PuttySession.SerialFlowControl);
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
      IEnumerable<XElement> PuttySessions;
      try {
        Root = XDocument.Load(Location).Root;
        if ( !Root.Elements(XML_ELEMENT_SESSIONS).Any() ) {
          yield break;
        }
        PuttySessions = Root.Element(XML_ELEMENT_SESSIONS).Elements(XML_ELEMENT_SESSION);
      } catch ( Exception ex ) {
        Log.Write($"Unable to read data from XML file {Location} : {ex.Message}");
        yield break;
      }

      
      foreach ( IPuttySession SessionItem in _ConvertFromXml(PuttySessions) ) {
        yield return SessionItem;
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
