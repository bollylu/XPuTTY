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
    public static XName XML_ROOT => GetXName("Root");
    public static XName XML_ELEMENT_SESSIONS => GetXName("Sessions");

    public static XName XML_ELEMENT_SESSION = GetXName("Session");

    protected const string XML_ATTRIBUTE_GROUP_LEVEL1 = "GroupLevel1";
    protected const string XML_ATTRIBUTE_GROUP_LEVEL2 = "GroupLevel2";
    protected const string XML_ATTRIBUTE_SECTION = "Section";
    protected const string XML_ATTRIBUTE_PROTOCOL_TYPE = "Protocol";

    protected const string XML_ATTRIBUTE_HOSTNAME = "HostName";
    protected const string XML_ATTRIBUTE_PORT = "PortNumber";
    protected static XName XML_ELEMENT_SSH_REMOTE_COMMAND = GetXName("RemoteCommand");

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
    }
    public TPuttySessionSourceXml(string location) : base() {
      Location = location;
    }

    protected override void _Initialize() {
      SourceType = ESourceType.Xml;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    /// <summary>
    /// Convert one XML data session to a session
    /// </summary>
    /// <param name="session">The XML data</param>
    /// <returns>One converted session or an Empty session in case of error</returns>
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

      XElement SessionCredential = session.SafeReadElement(TCredential.XML_THIS_ELEMENT);
      if ( SessionCredential.HasAttributes ) {
        BaseSession.SetLocalCredential(new TCredential(SessionCredential, BaseSession));
      }

      if ( Protocol.IsSerial ) {
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

      #region --- Session with host and port --------------------------------------------
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
      #endregion --- Session with host and port --------------------------------------------
    }

    /// <summary>
    /// Convert a list of XML data sessions to a list of sessions
    /// </summary>
    /// <param name="sessions">The list of XML data sessions</param>
    /// <returns>A list of sessions</returns>
    protected IEnumerable<IPuttySession> _ConvertFromXml(IEnumerable<XElement> sessions) {
      if ( sessions == null || !sessions.Any() ) {
        yield break;
      }
      foreach ( XElement SessionItem in sessions ) {
        yield return _ConvertFromXml(SessionItem);
      }
      yield break;
    }

    /// <summary>
    /// Convert one session to one XML data session
    /// </summary>
    /// <param name="session">The source session</param>
    /// <returns>The XML data session</returns>
    protected XElement _ConvertToXml(IPuttySession session) {

      XElement RetVal = new XElement(XML_ELEMENT_SESSION);

      string StrippedName = session.CleanName.Replace($"[{session.GroupLevel1}]", "")
                                             .Replace($"[{session.GroupLevel2}]", "")
                                             .Replace($"{{{session.Section}}}", "");
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, StrippedName);

      RetVal.SetAttributeValue(XML_ATTRIBUTE_PROTOCOL_TYPE, session.Protocol);

      if ( session.Credential != null ) {
        RetVal.Add(session.Credential.ToXml());
      }

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

    /// <summary>
    /// Convert a list of sessions to a list of XML data session
    /// </summary>
    /// <param name="sessions">The source sessions</param>
    /// <returns>The enumeration of XML data session</returns>
    protected IEnumerable<XElement> _ConvertToXml(IEnumerable<IPuttySession> sessions) {
      #region === Validate parameters ===
      if ( sessions == null ) {
        yield break;
      }
      if ( !sessions.Any() ) {
        yield break;
      }
      #endregion === Validate parameters ===
      foreach ( IPuttySession PuttySessionItem in sessions ) {
        yield return _ConvertToXml(PuttySessionItem);
      }
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Read data --------------------------------------------
    /// <summary>
    /// Get the list of available sessions
    /// </summary>
    /// <returns>An enumeration of tuples (session name, session protocol)</returns>
    protected override IEnumerable<(string, TPuttyProtocol)> _GetSessionList() {

      XElement Root = LoadXml();
      if ( Root == null ) {
        yield break;
      }

      foreach ( XElement SessionItem in Root.Elements(XML_ELEMENT_SESSIONS) ) {
        string SessionName = SessionItem.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");
        TPuttyProtocol SessionProtocol = SessionItem.SafeReadAttribute<string>(XML_ATTRIBUTE_PROTOCOL_TYPE, "Unknown");
        yield return (SessionName, SessionProtocol);
      }
    }

    /// <summary>
    /// Read one session based on Name and Protocol
    /// </summary>
    /// <param name="name">The name of the requested session</param>
    /// <param name="protocol">The protocol</param>
    /// <returns>The sessions converted from XML or an Empty session in case of error</returns>
    protected override IPuttySession _ReadSession(string name, TPuttyProtocol protocol) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(name) ) {
        Log.Write($"Unable to get the sessions list from {Location} : Name is missing or invalid");
        return TPuttySession.Empty;
      }
      if ( protocol.IsUnknown ) {
        Log.Write($"Unable to get the sessions list from {Location} : Protocol is unknown");
        return TPuttySession.Empty;
      }
      #endregion === Validate parameters ===

      XElement Root = LoadXml();
      if ( Root == null ) {
        return TPuttySession.Empty;
      }

      IEnumerable<XElement> PuttySessions = Root.Elements(XML_ELEMENT_SESSIONS);
      XElement RequestedSession = PuttySessions.FirstOrDefault(x => x.Attribute(XML_ATTRIBUTE_NAME).Value == name && x.Attribute(XML_ATTRIBUTE_PROTOCOL_TYPE).Value == protocol.ToString());

      if ( RequestedSession == null ) {
        return TPuttySession.Empty;
      }

      return _ConvertFromXml(RequestedSession);

    }

    /// <summary>
    /// Read all sessions
    /// </summary>
    /// <returns>An enumeration of the sessions</returns>
    protected override IEnumerable<IPuttySession> _ReadSessions() {
      XElement Root = LoadXml();
      if ( Root == null ) {
        yield break;
      }

      IEnumerable<XElement> XmlPuttySessions = Root.Element(XML_ELEMENT_SESSIONS).Elements(XML_ELEMENT_SESSION);

      bool NeedToSecure = false;
      IPuttySession[] PuttySessions = _ConvertFromXml(XmlPuttySessions).ToArray();

      foreach ( IPuttySession SessionItem in PuttySessions ) {
        if ( SessionItem.Credential != null && !SessionItem.Credential.XmlSecure ) {
          SessionItem.Credential.SetSecure(true);
          NeedToSecure = true;
        }
        yield return SessionItem;
      }
      if ( NeedToSecure ) {
        _SaveSessions(PuttySessions);
      }
    }

    protected override IEnumerable<TPuttySessionGroup> _ReadGroups() {
      XElement Root = LoadXml();
      if ( Root == null ) {
        yield break;
      }
      throw new NotImplementedException();
    }
    #endregion --- Read data --------------------------------------------

    #region --- Save data --------------------------------------------
    protected override void _SaveSession(IPuttySession session) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      SaveXml(_ConvertToXml(session));

    }

    protected override void _SaveSessions(IEnumerable<IPuttySession> sessions) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      XElement SessionsToSave = new XElement(XML_ELEMENT_SESSIONS);
      foreach ( XElement SessionItem in _ConvertToXml(sessions) ) {
        SessionsToSave.Add(SessionItem);
      }
      SaveXml(SessionsToSave);
    }
    #endregion --- Save data --------------------------------------------


    #region Xml IO
    public virtual bool SaveXml(XElement xmlData, string storageLocation = "") {
      #region Validate parameters
      if ( !string.IsNullOrWhiteSpace(storageLocation) ) {
        Location = storageLocation;
      }

      if ( xmlData == null ) {
        throw new ArgumentNullException(nameof(xmlData));
      }
      #endregion Validate parameters
      XDocument XmlFile = new XDocument {
        Declaration = new XDeclaration("1.0", Encoding.UTF8.EncodingName, "true")
      };
      XElement Root = new XElement(XML_ROOT);
      Root.Add(xmlData);
      XmlFile.Add(Root);
      try {
        Log.Write($"Saving data {this.GetType().Name} to file {Location} ...");
        XmlFile.Save(Location);
        Log.Write("SaveXml successful");
        return true;
      } catch ( Exception ex ) {
        Log.Write($"Unable to save information to file {Location} : {ex.Message}", ErrorLevel.Error);
        return false;
      }
    }

    public virtual XElement LoadXml(string storageLocation = "") {
      #region Validate parameters
      if ( !string.IsNullOrWhiteSpace(storageLocation) ) {
        Location = storageLocation;
      }
      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to read information from file {Location} : incorrect or missing filename", ErrorLevel.Error);
        return null;
      }
      #endregion Validate parameters
      XDocument XmlFile;
      try {
        Log.Write("Reading file content...");
        XmlFile = XDocument.Load(Location);

        Log.Write("Parsing content...");
        XElement Root = XmlFile.Root;
        if ( Root == null ) {
          Log.Write("unable to read config file content");
          return null;
        }

        Log.Write("LoadXml Sucessfull");
        return Root;
      } catch ( Exception ex ) {
        Log.Write($"Unable to read information from file {Location} : {ex.Message}", ErrorLevel.Error);
        return null;
      }
    }
    #endregion Xml IO
  }
}
