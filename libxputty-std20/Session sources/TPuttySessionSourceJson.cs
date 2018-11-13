using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BLTools;
using BLTools.Json;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionSourceJson : TPuttySessionSource {

    #region --- Constants --------------------------------------------
    protected const string JSON_ELEMENT_SESSIONS = "Sessions";

    protected const string JSON_ELEMENT_SESSION = "Session";
    protected const string JSON_ATTRIBUTE_NAME = "Name";
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
    public TPuttySessionSourceJson() : base() {
      SourceType = ESourceType.Json;
    }
    public TPuttySessionSourceJson(string location) : base() {
      SourceType = ESourceType.Json;
      Location = location;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    protected IPuttySession _ConvertFromJson(IJsonValue session) {

      return TPuttySession.Empty;

    }

    protected IEnumerable<IPuttySession> _ConvertFromJson(IEnumerable<IJsonValue> sessions) {
      if ( sessions == null || !sessions.Any() ) {
        yield break;
      }
      foreach ( IJsonValue SessionItem in sessions ) {
        yield return _ConvertFromJson(SessionItem);
      }
      yield break;
    }

    protected IJsonValue _ConvertToJson(IPuttySession session) {

      return JsonNull.Default;
      
    }

    protected IJsonValue _ConvertToJson(IEnumerable<IPuttySession> sessions) {
      //foreach ( IPuttySession PuttySessionItem in sessions ) {
      //  RetVal.Add(_ConvertToJson(PuttySessionItem));
      //}
      //return RetVal;
      return JsonNull.Default;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    protected override IEnumerable<(string, TPuttyProtocol)> _GetSessionList() {
      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        yield break;
      }

      JsonArray Sessions;
      try {
        JsonPair Root = JsonValue.Parse(File.ReadAllText(Location)) as JsonPair;
        if ( Root.Key != JSON_ELEMENT_SESSIONS ) {
          throw new ApplicationException("Json file content is invalid");
        }
        Sessions = Root.ArrayContent;
      } catch ( Exception ex ) {
        Log.Write($"Unable to read data from Json file {Location} : {ex.Message}");
        yield break;
      }

      foreach ( JsonObject JsonItem in Sessions) {
        string SessionName = JsonItem.SafeGetValueSingle<string>(JSON_ATTRIBUTE_NAME);
        TPuttyProtocol SessionProtocol = JsonItem.SafeGetValueSingle<string>(JSON_ATTRIBUTE_PROTOCOL_TYPE);
        yield return (SessionName, SessionProtocol);
      }

    }

    protected override IPuttySession _ReadSession(string name, TPuttyProtocol protocol) {

      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        return TPuttySession.Empty;
      }

      JsonArray Sessions;
      try {
        JsonPair Root = JsonValue.Parse(File.ReadAllText(Location)) as JsonPair;
        if ( Root.Key != JSON_ELEMENT_SESSIONS ) {
          throw new ApplicationException("Json file content is invalid");
        }
        Sessions = Root.ArrayContent;
      } catch ( Exception ex ) {
        Log.Write($"Unable to read data from Json file {Location} : {ex.Message}");
        return TPuttySession.Empty;
      }

      //JsonObject RequestedSession = Sessions.Cast<JsonPair>().FirstOrDefault(x => x.SafeGetValueFirst._ATTRIBUTE_NAME).Value == name && x.Attribute(XML_ATTRIBUTE_PROTOCOL_TYPE).Value == protocol.ToString());

      //if ( RequestedSession == null ) {
        return TPuttySession.Empty;
      //}

      //return _ConvertFromJson(RequestedSession);

    }

    protected override IEnumerable<IPuttySession> _ReadSessions() {
      if ( !File.Exists(Location) ) {
        Log.Write($"Unable to get the sessions list from {Location} : File is missing or access is denied");
        yield break;
      }

      
    }

    protected override void _SaveSession(IPuttySession session) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      

    }

    protected override void _SaveSessions(IEnumerable<IPuttySession> sessions) {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Location) ) {
        return;
      }
      #endregion === Validate parameters ===

      
    }

  }
}
