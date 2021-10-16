using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BLTools;
using BLTools.Json;

namespace libxputty {
  public class TSourceSessionPuttyJson : ASourceSession {

    #region --- Constants --------------------------------------------
    protected const string JSON_ELEMENT_SESSIONS = "Sessions";

    protected const string JSON_ELEMENT_SESSION = "Session";
    protected const string JSON_ATTRIBUTE_NAME = nameof(Name);
    protected const string JSON_ATTRIBUTE_GROUP_LEVEL1 = "GroupLevel1";
    protected const string JSON_ATTRIBUTE_GROUP_LEVEL2 = "GroupLevel2";
    protected const string JSON_ATTRIBUTE_SECTION = "Section";
    protected const string JSON_ATTRIBUTE_PROTOCOL_TYPE = "Protocol";

    protected const string JSON_ATTRIBUTE_HOSTNAME = "HostName";
    protected const string JSON_ATTRIBUTE_PORT = "PortNumber";
    protected const string JSON_ELEMENT_SSH_REMOTE_COMMAND = "RemoteCommand";

    protected const string JSON_ATTRIBUTE_SERIAL_LINE = "SerialLine";
    protected const string JSON_ATTRIBUTE_SERIAL_SPEED = "SerialSpeed";
    protected const string JSON_ATTRIBUTE_SERIAL_DATA_BITS = "SerialDataBits";
    protected const string JSON_ATTRIBUTE_SERIAL_PARITY = "SerialParity";
    protected const string JSON_ATTRIBUTE_SERIAL_STOP_BITS = "SerialStopHalfBits";
    protected const string JSON_ATTRIBUTE_SERIAL_FLOW_CONTROL = "SerialFlowControl";

    public const string DATASOURCE_PREFIX = "json";

    #endregion --- Constants --------------------------------------------

    public override string DataSourceName => $@"{DATASOURCE_PREFIX}://{Location ?? ""}";

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TSourceSessionPuttyJson() : base() {
      SourceType = ESourceType.Json;
    }
    public TSourceSessionPuttyJson(string location) : base() {
      SourceType = ESourceType.Json;
      Location = location;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    protected ISessionPutty _ConvertFromJson(IJsonValue session) {

      return ASessionPutty.Empty;

    }

    protected IEnumerable<ISessionPutty> _ConvertFromJson(IEnumerable<IJsonValue> sessions) {
      if ( sessions == null || !sessions.Any() ) {
        yield break;
      }
      foreach ( IJsonValue SessionItem in sessions ) {
        yield return _ConvertFromJson(SessionItem);
      }
      yield break;
    }

    protected IJsonValue _ConvertToJson(ISessionPutty session) {

      return JsonNull.Default;
      
    }

    protected IJsonValue _ConvertToJson(IEnumerable<ISessionPutty> sessions) {
      //foreach ( IPuttySession PuttySessionItem in sessions ) {
      //  RetVal.Add(_ConvertToJson(PuttySessionItem));
      //}
      //return RetVal;
      return JsonNull.Default;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    protected override IEnumerable<(string, TPuttyProtocol)> _GetSessionList() {
      if ( !File.Exists(Location) ) {
        LogError($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        yield break;
      }

      throw new NotImplementedException();
      //JsonArray Sessions;
      //try {
      //  JsonPair Root = JsonValue.Parse(File.ReadAllText(Location)) as JsonPair;
      //  if ( Root.Key != JSON_ELEMENT_SESSIONS ) {
      //    throw new ApplicationException("Json file content is invalid");
      //  }
      //  Sessions = Root.ArrayContent;
      //} catch ( Exception ex ) {
      //  LogError($"Unable to read data from Json file {Location} : {ex.Message}");
      //  yield break;
      //}

      //foreach ( JsonObject JsonItem in Sessions) {
      //  string SessionName = JsonItem.SafeGetValueSingle<string>(JSON_ATTRIBUTE_NAME);
      //  TPuttyProtocol SessionProtocol = JsonItem.SafeGetValueSingle<string>(JSON_ATTRIBUTE_PROTOCOL_TYPE);
      //  yield return (SessionName, SessionProtocol);
      //}

    }

    protected override ISessionPutty _ReadSession(string name, TPuttyProtocol protocol) {

      if ( !File.Exists(Location) ) {
        LogError($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        return ASessionPutty.Empty;
      }

      throw new NotImplementedException();

      //JsonArray Sessions;
      //try {
      //  JsonPair Root = JsonValue.Parse(File.ReadAllText(Location)) as JsonPair;
      //  if ( Root.Key != JSON_ELEMENT_SESSIONS ) {
      //    throw new ApplicationException("Json file content is invalid");
      //  }
      //  Sessions = Root.ArrayContent;
      //} catch ( Exception ex ) {
      //  LogError($"Unable to read data from Json file {Location} : {ex.Message}");
      //  return TPuttySession.Empty;
      //}

      ////JsonObject RequestedSession = Sessions.Cast<JsonPair>().FirstOrDefault(x => x.SafeGetValueFirst._ATTRIBUTE_NAME).Value == name && x.Attribute(XML_ATTRIBUTE_PROTOCOL_TYPE).Value == protocol.ToString());

      ////if ( RequestedSession == null ) {
      //  return TPuttySession.Empty;
      ////}

      ////return _ConvertFromJson(RequestedSession);

    }

    protected override IEnumerable<ISessionPutty> _ReadSessions() {
      if ( !File.Exists(Location) ) {
        LogError($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        yield break;
      }

      
    }

    protected override void _SaveSession(ISession session) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      

    }

    protected override void _SaveSessions(IEnumerable<ISession> sessions) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      
    }

    protected override IEnumerable<TPuttySessionGroup> _ReadGroups() {
      throw new NotImplementedException();
    }
  }
}
