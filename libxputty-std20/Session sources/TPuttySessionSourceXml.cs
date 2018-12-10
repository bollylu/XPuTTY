using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using BLTools;

using libxputty_std20.Interfaces;
using static libxputty_std20.LocalExtensions;

namespace libxputty_std20 {
  public class TPuttySessionSourceXml : TPuttySessionSource {

    #region --- Constants --------------------------------------------
    public static XName XML_ROOT => GetXName("Root");
    public static XName XML_ELEMENT_GROUP => GetXName("Group");

    public static XName XML_ELEMENT_SESSIONS => GetXName("Sessions");

    public static XName XML_ELEMENT_SESSION = GetXName("Session");
    public const string XML_ATTRIBUTE_ID = "Id";
    public const string ROOT_GROUP_ID = "";

    public static XName XML_ELEMENT_DESCRIPTION = GetXName("Description");
    public static XName XML_ELEMENT_COMMENT = GetXName("Comment");

    public const string XML_ATTRIBUTE_SESSION_TYPE = "SessionType";

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

    public override string DataSourceName => $@"{DATASOURCE_PREFIX}://{StorageLocation ?? ""}";

    #region --- Cache --------------------------------------------
    public XElement XmlDataCache;

    public IEnumerable<IPuttySessionsGroup> ProcessedDataCache {
      get {
        if ( XmlDataCache == null ) {
          return null;
        }

        if ( _ProcessedDataCache == null ) {
          _ProcessedDataCache = _ConvertGroupsFromXml(XmlDataCache.Elements(XML_ELEMENT_GROUP));
        }
        return _ProcessedDataCache;
      }
    }
    private IEnumerable<IPuttySessionsGroup> _ProcessedDataCache;

    public IPuttySessionsGroup RootGroup {
      get {
        if ( ProcessedDataCache == null ) {
          return null;
        }
        return ProcessedDataCache.SingleOrDefault();
      }
    }
    #endregion --- Cache -----------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSourceXml() : base() {
    }
    public TPuttySessionSourceXml(string location) : base() {
      StorageLocation = location;
    }

    protected override void _Initialize() {
      SourceType = ESourceType.Xml;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters for IputtySession -------------------------------------------------------------------------------------
    /// <summary>
    /// Convert one XML data session to a session
    /// </summary>
    /// <param name="session">The XML data</param>
    /// <returns>One converted session or an Empty session in case of error</returns>
    protected IPuttySession _ConvertSessionFromXml(XElement session) {

      string Name = session.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");
      TPuttyProtocol Protocol = session.SafeReadAttribute<string>(XML_ATTRIBUTE_PROTOCOL_TYPE, EPuttyProtocol.Unknown.ToString());

      if ( string.IsNullOrWhiteSpace(Name) ) {
        Log.Write($"Error while converting XML Putty session : Name is empty or invalid");
        return TPuttySession.Empty;
      }

      TPuttySession BaseSession = new TPuttySession(Name) {
        ID = session.SafeReadAttribute<string>(XML_ATTRIBUTE_ID, ""),
        Description = session.SafeReadElementValue<string>(XML_ELEMENT_DESCRIPTION, ""),
        Comment = session.SafeReadElementValue<string>(XML_ELEMENT_COMMENT, ""),
        SessionType = (ESessionType)Enum.Parse(typeof(ESessionType), session.SafeReadAttribute<string>(XML_ATTRIBUTE_SESSION_TYPE, ESessionType.Auto.ToString()), true),
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
          SerialParity = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SERIAL_PARITY, "n"),
          SerialFlowControl = session.SafeReadAttribute<string>(XML_ATTRIBUTE_SERIAL_FLOW_CONTROL, "")
        };
        return NewSession;
      }

      #region --- Session with host and port --------------------------------------------
      ISessionTypeNetwork SessionHAP;

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
    protected IEnumerable<IPuttySession> _ConvertSessionsFromXml(IEnumerable<XElement> sessions) {
      if ( sessions == null || !sessions.Any() ) {
        yield break;
      }
      foreach ( XElement SessionItem in sessions ) {
        yield return _ConvertSessionFromXml(SessionItem);
      }
      yield break;
    }

    /// <summary>
    /// Convert one session to one XML data session
    /// </summary>
    /// <param name="session">The source session</param>
    /// <returns>The XML data session</returns>
    protected XElement _ConvertSessionToXml(IPuttySession session) {

      XElement RetVal = new XElement(XML_ELEMENT_SESSION);

      if ( session.SessionType != ESessionType.Auto ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SESSION_TYPE, session.SessionType.ToString());
      }

      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);

      RetVal.SetAttributeValue(XML_ATTRIBUTE_ID, session.ID);

      if ( session.Description != "" ) {
        RetVal.SetElementValue(XML_ELEMENT_DESCRIPTION, session.Description);
      }

      if ( session.Comment != "" ) {
        RetVal.SetElementValue(XML_ELEMENT_COMMENT, session.Comment);
      }

      RetVal.SetAttributeValue(XML_ATTRIBUTE_PROTOCOL_TYPE, session.Protocol);

      if ( session.Credential != null ) {
        RetVal.Add(session.Credential.ToXml());
      }

      if ( session is ISessionTypeNetwork SessionTypeNetwork ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, SessionTypeNetwork.HostName);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, SessionTypeNetwork.Port);
      }

      if ( session is TPuttySessionSerial SessionTypeSerial ) {
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_LINE, SessionTypeSerial.SerialLine);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_SPEED, SessionTypeSerial.SerialSpeed);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_DATA_BITS, SessionTypeSerial.SerialDataBits);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_STOP_BITS, SessionTypeSerial.SerialStopBits);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_PARITY, SessionTypeSerial.SerialParity);
        RetVal.SetAttributeValue(XML_ATTRIBUTE_SERIAL_FLOW_CONTROL, SessionTypeSerial.SerialFlowControl);
      }

      if ( !string.IsNullOrWhiteSpace(session.RemoteCommand) ) {
        RetVal.SetElementValue(XML_ELEMENT_SSH_REMOTE_COMMAND, session.RemoteCommand);
      }

      return RetVal;
    }

    /// <summary>
    /// Convert a list of sessions to a list of XML data session
    /// </summary>
    /// <param name="sessions">The source sessions</param>
    /// <returns>The enumeration of XML data session</returns>
    protected IEnumerable<XElement> _ConvertSessionsToXml(IEnumerable<IPuttySession> sessions) {
      #region === Validate parameters ===
      if ( sessions == null ) {
        yield break;
      }
      if ( !sessions.Any() ) {
        yield break;
      }
      #endregion === Validate parameters ===
      foreach ( IPuttySession PuttySessionItem in sessions ) {
        yield return _ConvertSessionToXml(PuttySessionItem);
      }
    }
    #endregion --- Converters for IputtySession ----------------------------------------------------------------------------------

    #region --- Converters for IPuttySessionsGroup -------------------------------------------------------------------------------------
    /// <summary>
    /// Convert one XML data group to a group
    /// </summary>
    /// <param name="group">The XML data</param>
    /// <returns>One converted group</returns>
    protected IPuttySessionsGroup _ConvertGroupFromXml(XElement group) {
      if ( group == null ) {
        throw new ArgumentNullException(nameof(group), "Missing or invalid xml group for conversion");
      }

      string Name = group.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");

      IPuttySessionsGroup RetVal = new TPuttySessionGroup(Name) {
        ID = group.SafeReadAttribute<string>(XML_ATTRIBUTE_ID, "")
      };

      foreach ( XElement XmlGroupItem in group.Elements(XML_ELEMENT_GROUP) ) {
        RetVal.AddOrUpdateGroup(_ConvertGroupFromXml(XmlGroupItem));
      }

      foreach ( XElement XmlSessionItem in group.Elements(XML_ELEMENT_SESSION) ) {
        RetVal.AddOrUpdateSession(_ConvertSessionFromXml(XmlSessionItem));
      }
      return RetVal;

    }

    /// <summary>
    /// Convert a list of XML data groups to a list of groups
    /// </summary>
    /// <param name="groups">The list of XML data sessions</param>
    /// <returns>A list of groups</returns>
    protected IEnumerable<IPuttySessionsGroup> _ConvertGroupsFromXml(IEnumerable<XElement> groups) {
      if ( groups == null || !groups.Any() ) {
        yield break;
      }
      foreach ( XElement XmlGroupItem in groups ) {
        yield return _ConvertGroupFromXml(XmlGroupItem);
      }
      yield break;
    }

    /// <summary>
    /// Convert one session to one XML data session
    /// </summary>
    /// <param name="session">The source session</param>
    /// <returns>The XML data session</returns>
    protected XElement _ConvertGroupToXml(IPuttySessionsGroup group) {

      XElement RetVal = new XElement(XML_ELEMENT_GROUP);

      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_ID, group.ID);

      if ( group.Description != "" ) {
        RetVal.SetElementValue(XML_ELEMENT_DESCRIPTION, group.Description);
      }

      if ( group.Comment != "" ) {
        RetVal.SetElementValue(XML_ELEMENT_COMMENT, group.Comment);
      }

      if ( group.Credential != null ) {
        RetVal.Add(group.Credential.ToXml());
      }

      foreach ( XElement XmlSessionItem in _ConvertSessionsToXml(group.Sessions) ) {
        RetVal.Add(XmlSessionItem);
      }

      foreach ( XElement XmlGroupItem in _ConvertGroupsToXml(group.Groups) ) {
        RetVal.Add(XmlGroupItem);
      }

      return RetVal;
    }

    /// <summary>
    /// Convert a list of sessions to a list of XML data session
    /// </summary>
    /// <param name="sessions">The source sessions</param>
    /// <returns>The enumeration of XML data session</returns>
    protected IEnumerable<XElement> _ConvertGroupsToXml(IEnumerable<IPuttySessionsGroup> groups) {
      #region === Validate parameters ===
      if ( groups == null ) {
        yield break;
      }
      if ( !groups.Any() ) {
        yield break;
      }
      #endregion === Validate parameters ===
      foreach ( IPuttySessionsGroup PuttySessionsGroupItem in groups ) {
        yield return _ConvertGroupToXml(PuttySessionsGroupItem);
      }
    }
    #endregion --- Converters for IPuttySessionsGroup ----------------------------------------------------------------------------------

    //#region --- Read data --------------------------------------------
    ///// <summary>
    ///// Get the list of available sessions from a group
    ///// </summary>
    ///// <returns>An enumeration of tuples (session name, session protocol)</returns>
    //protected IEnumerable<(string, TPuttyProtocol)> _GetSessionList(XElement group, bool recurse = false) {

    //  if ( group == null ) {
    //    yield break;
    //  }

    //  foreach ( XElement SessionItem in group.Elements(XML_ELEMENT_SESSION) ) {
    //    string SessionName = SessionItem.SafeReadAttribute<string>(XML_ATTRIBUTE_NAME, "");
    //    TPuttyProtocol SessionProtocol = SessionItem.SafeReadAttribute<string>(XML_ATTRIBUTE_PROTOCOL_TYPE, "Unknown");
    //    yield return (SessionName, SessionProtocol);
    //  }
    //  if ( recurse ) {
    //    foreach ( XElement GroupItem in group.Elements(XML_ELEMENT_GROUP) ) {
    //      foreach ( (string, TPuttyProtocol) TupleItem in _GetSessionList(GroupItem, recurse) ) {
    //        yield return TupleItem;
    //      }
    //    }
    //  }
    //}

    ///// <summary>
    ///// Read one session based on Name and Protocol
    ///// </summary>
    ///// <param name="name">The name of the requested session</param>
    ///// <param name="protocol">The protocol</param>
    ///// <returns>The sessions converted from XML or an Empty session in case of error</returns>
    //protected IPuttySession _ReadSession(XElement group, string id, bool recurse = true) {
    //  if ( group == null || string.IsNullOrWhiteSpace(id) ) {
    //    return TPuttySession.Empty;
    //  }

    //  IEnumerable<XElement> PuttySessions = group.Elements(XML_ELEMENT_SESSIONS);
    //  XElement RequestedSession = PuttySessions.FirstOrDefault(x => x.Attribute(XML_ATTRIBUTE_ID).Value == id);

    //  if ( RequestedSession == null ) {
    //    if ( recurse ) {
    //      foreach ( XElement GroupItem in group.Elements(XML_ELEMENT_GROUP) ) {
    //        IPuttySession SubSession = _ReadSession(GroupItem, id, recurse);
    //        if ( SubSession.Name != EMPTY ) {
    //          return SubSession;
    //        }
    //      }
    //    }
    //    return TPuttySession.Empty;
    //  }

    //  return _ConvertFromXml(RequestedSession);

    //}

    ///// <summary>
    ///// Read all sessions
    ///// </summary>
    ///// <returns>An enumeration of the sessions</returns>
    //protected override IEnumerable<IPuttySession> _ReadSessions() {
    //  XElement Root = LoadXml();
    //  if ( Root == null ) {
    //    yield break;
    //  }

    //  IEnumerable<XElement> XmlPuttySessions = Root.Element(XML_ELEMENT_SESSIONS).Elements(XML_ELEMENT_SESSION);

    //  bool NeedToSecure = false;
    //  IPuttySession[] PuttySessions = _ConvertFromXml(XmlPuttySessions).ToArray();

    //  foreach ( IPuttySession SessionItem in PuttySessions ) {
    //    if ( SessionItem.Credential != null && !SessionItem.Credential.XmlSecure ) {
    //      SessionItem.Credential.SetSecure(true);
    //      NeedToSecure = true;
    //    }
    //    yield return SessionItem;
    //  }
    //  if ( NeedToSecure ) {
    //    _SaveSessions(PuttySessions);
    //  }
    //}

    //protected override IEnumerable<TPuttySessionGroup> _ReadGroups() {
    //  XElement Root = LoadXml();
    //  if ( Root == null ) {
    //    yield break;
    //  }
    //  throw new NotImplementedException();
    //}
    //#endregion --- Read data --------------------------------------------

    //#region --- Save data --------------------------------------------
    //protected override void _SaveSession(IPuttySession session) {
    //  #region === Validate parameters ===
    //  if ( string.IsNullOrWhiteSpace(Location) ) {
    //    return;
    //  }
    //  #endregion === Validate parameters ===

    //  SaveXml(_ConvertToXml(session));

    //}

    //protected override void _SaveSessions(IEnumerable<IPuttySession> sessions) {
    //  #region === Validate parameters ===
    //  if ( string.IsNullOrWhiteSpace(Location) ) {
    //    return;
    //  }
    //  #endregion === Validate parameters ===

    //  XElement SessionsToSave = new XElement(XML_ELEMENT_SESSIONS);
    //  foreach ( XElement SessionItem in _ConvertToXml(sessions) ) {
    //    SessionsToSave.Add(SessionItem);
    //  }
    //  SaveXml(SessionsToSave);
    //}
    //#endregion --- Save data --------------------------------------------

    #region Xml IO
    public bool SaveXml(XElement xmlData, string storageLocation = "") {
      #region Validate parameters
      if ( !string.IsNullOrWhiteSpace(storageLocation) ) {
        StorageLocation = storageLocation;
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
        Log.Write($"Saving data {this.GetType().Name} to file {StorageLocation} ...");
        XmlFile.Save(StorageLocation);
        Log.Write("SaveXml successful");
        return true;
      } catch ( Exception ex ) {
        Log.Write($"Unable to save information to file {StorageLocation} : {ex.Message}", ErrorLevel.Error);
        return false;
      }
    }

    public XElement LoadXml(string storageLocation = "") {
      #region Validate parameters
      if ( !string.IsNullOrWhiteSpace(storageLocation) ) {
        StorageLocation = storageLocation;
      }
      if ( !File.Exists(StorageLocation) ) {
        Log.Write($"Unable to read information from file {StorageLocation} : incorrect or missing filename", ErrorLevel.Error);
        return null;
      }
      #endregion Validate parameters

      XDocument XmlFile;
      try {
        Log.Write("Reading file content...");
        XmlFile = XDocument.Load(StorageLocation);

        Log.Write("Parsing content...");
        XmlDataCache = XmlFile.Root;
        if ( XmlDataCache == null ) {
          Log.Write("unable to read config file content");
          return null;
        }
        Log.Write("LoadXml Sucessfull");
        return XmlDataCache;
      } catch ( Exception ex ) {
        Log.Write($"Unable to read information from file {StorageLocation} : {ex.Message}", ErrorLevel.Error);
        return null;
      }
    }

    public void ClearCache() {
      XmlDataCache = null;
      _ProcessedDataCache = null;
    }

    #endregion Xml IO

    public override IEnumerable<IPuttySessionsGroup> GetGroupsFrom(string groupId = ROOT_GROUP_ID, bool recurse = false) {
      #region --- Is there any data --------------------------------------------
      if ( ProcessedDataCache == null ) {
        XmlDataCache = LoadXml();
      }
      if ( RootGroup == null ) {
        yield break;
      }
      #endregion --- Is there any data -----------------------------------------

      IPuttySessionsGroup FakeRootGroup;

      if ( groupId == ROOT_GROUP_ID ) {
        FakeRootGroup = RootGroup;
      } else {
        FakeRootGroup = RootGroup.GetGroup(groupId, recurse);
        if ( FakeRootGroup == null ) {
          yield break;
        }
      }

      foreach ( IPuttySessionsGroup GroupItem in FakeRootGroup.GetAllGroups(recurse) ) {
        yield return GroupItem;
      }

    }

    public override IPuttySessionsGroup GetGroup(string groupId, bool recurse = true) {
      if ( ProcessedDataCache == null ) {
        XmlDataCache = LoadXml();
      }
      if ( ProcessedDataCache == null ) {
        return null;
      }
      if ( !ProcessedDataCache.Any() ) {
        return null;
      }

      return RootGroup.GetAllGroups(recurse).FirstOrDefault(x => x.ID == groupId);
    }

    public override IEnumerable<(string, TPuttyProtocol)> GetSessionsList(string groupId, bool recurse) {
      throw new NotImplementedException();
    }

    public override IEnumerable<(string, TPuttyProtocol)> GetSessionsList(IPuttySessionsGroup group, bool recurse) {
      throw new NotImplementedException();
    }

    public override IEnumerable<IPuttySession> GetSessions(string groupId, bool recurse = false) {
      throw new NotImplementedException();
    }

    public override IEnumerable<IPuttySession> GetSessions(IPuttySessionsGroup group, bool recurse = false) {
      throw new NotImplementedException();
    }

    public override IPuttySession GetSession(IPuttySessionsGroup group, string sessionId, bool recurse = true) {
      throw new NotImplementedException();
    }

    public override void SaveGroup(IPuttySessionsGroup group) {
      throw new NotImplementedException();
    }

    public override void UpdateSession(IPuttySession session) {
      throw new NotImplementedException();
    }

  }
}
