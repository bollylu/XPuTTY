using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using BLTools;

namespace libxputty {
  public class TSourceSessionPuttyXml : ASourceSession {

    #region --- Constants --------------------------------------------
    public static readonly XName XML_ROOT = GetXName("Root");
    public static readonly XName XML_ELEMENT_SESSIONS = GetXName("Sessions");

    public static readonly XName XML_ELEMENT_SESSION = GetXName("Session");
    public static readonly XName XML_ELEMENT_DESCRIPTION = GetXName("Description");
    public static readonly XName XML_ELEMENT_COMMENT = GetXName("Comment");

    protected const string XML_ATTRIBUTE_SESSION_TYPE = "SessionType";

    protected const string XML_ATTRIBUTE_GROUP_LEVEL1 = "GroupLevel1";
    protected const string XML_ATTRIBUTE_GROUP_LEVEL2 = "GroupLevel2";
    protected const string XML_ATTRIBUTE_SECTION = "Section";
    protected const string XML_ATTRIBUTE_PROTOCOL_TYPE = "Protocol";

    protected const string XML_ATTRIBUTE_HOSTNAME = "HostName";
    protected const string XML_ATTRIBUTE_PORT = "PortNumber";
    protected static readonly XName XML_ELEMENT_SSH_REMOTE_COMMAND = GetXName("RemoteCommand");

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
    public TSourceSessionPuttyXml() : base() {
    }
    public TSourceSessionPuttyXml(string location) : base() {
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
    protected ISessionPutty _ConvertFromXml(XElement session) {

      string Name = session.SafeReadAttribute(XML_ATTRIBUTE_NAME, "");
      TPuttyProtocol Protocol = session.SafeReadAttribute(XML_ATTRIBUTE_PROTOCOL_TYPE, EPuttyProtocol.Unknown.ToString());

      if (string.IsNullOrWhiteSpace(Name)) {
        LogError($"Unable to convert XML Putty session : Name is empty or invalid");
        return ASessionPutty.Empty;
      }

      ISessionPutty BaseSession = ASessionPutty.BuildSessionPutty(Protocol.Value, Logger);
      BaseSession.Name = Name;
      BaseSession.Description = session.SafeReadElementValue(XML_ELEMENT_DESCRIPTION, "");
      BaseSession.Comment = session.SafeReadElementValue(XML_ELEMENT_COMMENT, "");
      BaseSession.SessionType = Enum.Parse<ESessionPuttyType>(session.SafeReadAttribute(XML_ATTRIBUTE_SESSION_TYPE, ESessionPuttyType.Auto.ToString()), true);
      BaseSession.GroupLevel1 = session.SafeReadAttribute(XML_ATTRIBUTE_GROUP_LEVEL1, LocalExtensions.EMPTY);
      BaseSession.GroupLevel2 = session.SafeReadAttribute(XML_ATTRIBUTE_GROUP_LEVEL2, LocalExtensions.EMPTY);
      BaseSession.Section = session.SafeReadAttribute(XML_ATTRIBUTE_SECTION, LocalExtensions.EMPTY);
      BaseSession.RemoteCommand = session.SafeReadElementValue(XML_ELEMENT_SSH_REMOTE_COMMAND, "");

      XElement SessionCredential = session.SafeReadElement(TCredential.XML_THIS_ELEMENT);

      if (SessionCredential.HasAttributes) {
        BaseSession.SetLocalCredential(new TCredential(SessionCredential, BaseSession));
      }

      if (BaseSession is TSessionPuttySerial Serial) {
        Serial.SerialLine = session.SafeReadAttribute(XML_ATTRIBUTE_SERIAL_LINE, "");
        Serial.SerialSpeed = session.SafeReadAttribute(XML_ATTRIBUTE_SERIAL_SPEED, 9600);
        Serial.SerialDataBits = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_DATA_BITS, 0);
        Serial.SerialStopBits = session.SafeReadAttribute<byte>(XML_ATTRIBUTE_SERIAL_STOP_BITS, 0);
        Serial.SerialParity = session.SafeReadAttribute(XML_ATTRIBUTE_SERIAL_PARITY, "n");
        Serial.SerialFlowControl = session.SafeReadAttribute(XML_ATTRIBUTE_SERIAL_FLOW_CONTROL, "");
        return BaseSession;
      }

      if (BaseSession is IHostAndPort SessionHAP) {
        SessionHAP.HostName = session.SafeReadAttribute(XML_ATTRIBUTE_HOSTNAME, "");
        SessionHAP.Port = session.SafeReadAttribute(XML_ATTRIBUTE_PORT, 0);
        return BaseSession;
      }

      return BaseSession;
    }

    /// <summary>
    /// Convert a list of XML data sessions to a list of sessions
    /// </summary>
    /// <param name="sessions">The list of XML data sessions</param>
    /// <returns>A list of sessions</returns>
    protected IEnumerable<ISessionPutty> _ConvertFromXml(IEnumerable<XElement> sessions) {
      if (sessions is null || sessions.IsEmpty()) {
        yield break;
      }
      foreach (XElement SessionItem in sessions) {
        yield return _ConvertFromXml(SessionItem);
      }
      yield break;
    }

    protected XElement _ConvertToXml(ISession session) {
      return _ConvertToXml(session as ISessionPutty);
    }

    /// <summary>
    /// Convert one session to one XML data session
    /// </summary>
    /// <param name="session">The source session</param>
    /// <returns>The XML data session</returns>
    protected XElement _ConvertToXml(ISessionPutty session) {

      XElement RetVal = new XElement(XML_ELEMENT_SESSION);

      if (session.SessionType != ESessionPuttyType.Auto) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SESSION_TYPE, session.SessionType.ToString());
      }
      string StrippedName = session.CleanName.Replace($"[{session.GroupLevel1}]", "")
                                             .Replace($"[{session.GroupLevel2}]", "")
                                             .Replace($"{{{session.Section}}}", "");
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, StrippedName);

      if (session.Description != "") {
        RetVal.SetElementValue(XML_ELEMENT_DESCRIPTION, session.Description);
      }

      if (session.Comment != "") {
        RetVal.SetElementValue(XML_ELEMENT_COMMENT, session.Comment);
      }

      RetVal.SetAttributeValue(XML_ATTRIBUTE_PROTOCOL_TYPE, session.Protocol);

      if (session.Credential is not null) {
        RetVal.Add(session.Credential.ToXml());
      }

      if (!session.GroupLevel1.IsEmpty()) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_GROUP_LEVEL1, session.GroupLevel1);
      }
      if (!session.GroupLevel2.IsEmpty()) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_GROUP_LEVEL2, session.GroupLevel2);
      }
      if (!session.Section.IsEmpty()) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SECTION, session.Section);
      }

      if (session is IHostAndPort SessionHAP) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, SessionHAP.HostName);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, SessionHAP.Port);
      }

      if (!string.IsNullOrWhiteSpace(session.RemoteCommand)) {
        RetVal.SetElementValue(XML_ELEMENT_SSH_REMOTE_COMMAND, session.RemoteCommand);
      }

      if (session is TSessionPuttySerial PuttySession) {
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
    protected IEnumerable<XElement> _ConvertToXml(IEnumerable<ISessionPutty> sessions) {
      #region === Validate parameters ===
      if (sessions == null) {
        yield break;
      }
      if (!sessions.Any()) {
        yield break;
      }
      #endregion === Validate parameters ===
      foreach (ISessionPutty PuttySessionItem in sessions) {
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
      if (Root == null) {
        yield break;
      }

      foreach (XElement SessionItem in Root.Elements(XML_ELEMENT_SESSIONS)) {
        string SessionName = SessionItem.SafeReadAttribute(XML_ATTRIBUTE_NAME, "");
        TPuttyProtocol SessionProtocol = SessionItem.SafeReadAttribute(XML_ATTRIBUTE_PROTOCOL_TYPE, "Unknown");
        yield return (SessionName, SessionProtocol);
      }
    }

    /// <summary>
    /// Read one session based on Name and Protocol
    /// </summary>
    /// <param name="name">The name of the requested session</param>
    /// <param name="protocol">The protocol</param>
    /// <returns>The sessions converted from XML or an Empty session in case of error</returns>
    protected override ISessionPutty _ReadSession(string name, TPuttyProtocol protocol) {
      #region === Validate parameters ===
      if (string.IsNullOrWhiteSpace(name)) {
        LogError($"Unable to get the sessions list from {Location} : Name is missing or invalid");
        return ASessionPutty.Empty;
      }
      if (protocol.IsUnknown) {
        LogError($"Unable to get the sessions list from {Location} : Protocol is unknown");
        return ASessionPutty.Empty;
      }
      #endregion === Validate parameters ===

      XElement Root = LoadXml();
      if (Root == null) {
        return ASessionPutty.Empty;
      }

      IEnumerable<XElement> PuttySessions = Root.Elements(XML_ELEMENT_SESSIONS);
      XElement RequestedSession = PuttySessions.FirstOrDefault(x => x.Attribute(XML_ATTRIBUTE_NAME).Value.Equals(name, StringComparison.InvariantCultureIgnoreCase) && 
                                                                    x.Attribute(XML_ATTRIBUTE_PROTOCOL_TYPE).Value == protocol.ToString());

      if (RequestedSession is null) {
        return ASessionPutty.Empty;
      }

      return _ConvertFromXml(RequestedSession);

    }

    /// <summary>
    /// Read all sessions
    /// </summary>
    /// <returns>An enumeration of the sessions</returns>
    protected override IEnumerable<ISessionPutty> _ReadSessions() {
      XElement Root = LoadXml();
      if (Root is null) {
        yield break;
      }

      IEnumerable<XElement> XmlPuttySessions = Root.Element(XML_ELEMENT_SESSIONS).Elements(XML_ELEMENT_SESSION);

      bool NeedToSecure = false;
      ISessionPutty[] PuttySessions = _ConvertFromXml(XmlPuttySessions).ToArray();

      foreach (ISessionPutty SessionItem in PuttySessions) {
        if (SessionItem.Credential is not null && !SessionItem.Credential.XmlSecure) {
          SessionItem.Credential.SetSecure(true);
          NeedToSecure = true;
        }
        yield return SessionItem;
      }
      if (NeedToSecure) {
        _SaveSessions(PuttySessions);
      }
    }

    protected override IEnumerable<TPuttySessionGroup> _ReadGroups() {
      XElement Root = LoadXml();
      if (Root is null) {
        yield break;
      }
      throw new NotImplementedException();
    }
    #endregion --- Read data --------------------------------------------

    #region --- Save data --------------------------------------------
    protected override void _SaveSession(ISession session) {
      #region === Validate parameters ===
      if (string.IsNullOrWhiteSpace(Location)) {
        return;
      }
      #endregion === Validate parameters ===

      SaveXml(_ConvertToXml(session));

    }

    protected override void _SaveSessions(IEnumerable<ISession> sessions) {
      #region === Validate parameters ===
      if (string.IsNullOrWhiteSpace(Location)) {
        return;
      }
      #endregion === Validate parameters ===

      XElement SessionsToSave = new XElement(XML_ELEMENT_SESSIONS);
      foreach (ISessionPutty SessionItem in sessions) {
        SessionsToSave.Add(_ConvertToXml(SessionItem));
      }
      SaveXml(SessionsToSave);
    }
    #endregion --- Save data --------------------------------------------

    #region Xml IO
    public virtual bool SaveXml(XElement xmlData, string storageLocation = "") {
      #region Validate parameters
      if (!string.IsNullOrWhiteSpace(storageLocation)) {
        Location = storageLocation;
      }

      if (xmlData == null) {
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
        Log($"Saving data {this.GetType().Name} to file {Location} ...");
        XmlFile.Save(Location);
        Log("SaveXml successful");
        return true;
      } catch (Exception ex) {
        LogError($"Unable to save information to file {Location} : {ex.Message}");
        return false;
      }
    }

    public virtual XElement LoadXml(string storageLocation = "") {
      #region Validate parameters
      if (!string.IsNullOrWhiteSpace(storageLocation)) {
        Location = storageLocation;
      }
      if (!File.Exists(Location)) {
        LogError($"Unable to read information from file {Location} : incorrect or missing filename");
        return null;
      }
      #endregion Validate parameters
      XDocument XmlFile;
      try {
        Log("Reading file content...");
        XmlFile = XDocument.Load(Location);

        Log("Parsing content...");
        XElement Root = XmlFile.Root;
        if (Root is null) {
          LogError("unable to read config file content");
          return null;
        }

        Log("LoadXml Sucessfull");
        return Root;
      } catch (Exception ex) {
        LogError($"Unable to read information from file {Location} : {ex.Message}");
        return null;
      }
    }
    #endregion Xml IO
  }
}
