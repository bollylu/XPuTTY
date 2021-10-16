using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using BLTools;
using BLTools.Diagnostic.Logging;

namespace libxputty {
  public abstract class ASession : ALoggable, ISession {

    #region XML constants
    public const string XML_ATTRIBUTE_NAME = nameof(Name);
    public const string XML_ATTRIBUTE_DESCRIPTION = nameof(Description);
    public const string XML_ATTRIBUTE_COMMENT = nameof(Comment);

    public static string DefaultXmlNamespace
    {
      get
      {
        return "http://easyputty.sharenet.be";
      }
    }

    public static string XmlNamespace
    {
      get
      {
        if (_XmlNamespace == null) {
          return DefaultXmlNamespace;
        } else {
          return _XmlNamespace;
        }
      }
      set
      {
        _XmlNamespace = value;
      }
    }
    protected static string _XmlNamespace;

    public static XName GetXName(string name = "") {
      if (string.IsNullOrEmpty(name)) {
        return XName.Get(XmlNamespace);
      } else {
        return XName.Get(name, XmlNamespace);
      }
    }

    public static XName GetXName(string name, string xmlNamespace) {
      if (string.IsNullOrEmpty(name)) {
        return XName.Get(xmlNamespace);
      } else {
        return XName.Get(name, xmlNamespace);
      }
    }
    #endregion XML constants

    #region --- IName --------------------------------------------
    /// <summary>
    /// Name of the item
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Description of the item
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// A comment
    /// </summary>
    public string Comment { get; set; } = "";
    #endregion --- IName --------------------------------------------

    #region --- IStorage --------------------------------------------
    /// <summary>
    /// The location where to save or load information
    /// </summary>
    public string StorageLocation { get; set; } = "";
    #endregion --- IStorage --------------------------------------------

    #region --- IParent --------------------------------------------
    public IParent Parent { get; set; }
    public T GetParent<T>() {
      if (Parent is null) {
        return default(T);
      }
      if (Parent.GetType().Name == typeof(T).Name) {
        return (T)Convert.ChangeType(Parent, typeof(T));
      }
      return Parent.GetParent<T>();
    }
    #endregion --- IParent --------------------------------------------

    #region --- ICredentialContainer --------------------------------------------
    /// <summary>
    /// The credential to use when executing an action
    /// </summary>
    public virtual ICredential Credential
    {
      get
      {
        // If local credential, use it
        if (_LocalCredential is not null) {
          return _LocalCredential;
        }

        // If a parent is ICredentialContainer, use it
        if (Parent is not null && Parent is ICredentialContainer ParentWithCredential) {
          return ParentWithCredential.Credential;
        }

        // No Credential available
        return null;
      }
    }
    protected ICredential _LocalCredential;

    public void SetLocalCredential(ICredential credential) {
      if (credential is null) {
        return;
      }
      if (credential.HasValue) {
        _LocalCredential = new TCredential(credential, this);
      }
    }

    public virtual void SetSecure(bool value, bool recurse = true) {
      if (_LocalCredential is not null) {
        _LocalCredential.SetSecure(value);
      }

      if (!recurse) {
        return;
      }

      PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
      if (ItemsProperty is null) {
        return;
      }

      if (ItemsProperty.GetValue(this) is not IEnumerable<object> LocalItems) {
        return;
      }

      foreach (ICredentialContainer CredentialContainerItem in LocalItems.OfType<ICredentialContainer>()) {
        CredentialContainerItem.SetSecure(value, recurse);
      }

    }

    public bool IsCredentialInherited => _LocalCredential is null && 
                                         Parent is not null && 
                                         Parent is ICredentialContainer;

    public virtual bool HasUnsecuredPassword
    {
      get
      {
        if (_LocalCredential is not null && !_LocalCredential.XmlSecure) {
          return true;
        }

        // If tested item has sub-items, test them too
        PropertyInfo ItemsProperty = this.GetType().GetProperty("Items");
        if (ItemsProperty is not null) {
          if (ItemsProperty.GetValue(this) is IEnumerable<object> LocalItems) {
            return LocalItems.OfType<ICredentialContainer>().Any(x => x.HasUnsecuredPassword);
          }
        }

        return false;
      }
    }
    #endregion --- ICredentialContainer --------------------------------------------

    public ESessionType SessionType { get; protected set; } = ESessionType.Unknown;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    protected ASession() { }
    protected ASession(ISession session) {
      if (session is null) {
        return;
      }
      Name = session.Name;
      Description = session.Description;
      Comment = session.Comment;
      StorageLocation = session.StorageLocation;
      Parent = session.Parent;
      _Initialize();
    }

    private bool _IsInitialized = false;
    private bool _IsInitializing = false;

    protected virtual void _Initialize() {
      if (_IsInitialized || _IsInitializing) {
        return;
      }
      _IsInitializing = true;

      try {

      } finally {
        _IsInitialized = true;
        _IsInitializing = false;
      }

    }
    public virtual void Dispose() {

    }

    public static ISession BuildSession(ESessionType sessionType, ILogger logger) {

      if (logger is null) {
        logger = ALogger.DEFAULT_LOGGER;
      }

      switch (sessionType) {

        case ESessionType.PuttySSH: {
            ISession RetVal = new TSessionPuttySsh();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case ESessionType.PuttyRaw: {
            ISession RetVal = new TSessionPuttyRaw();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case ESessionType.PuttyTelnet: {
            ISession RetVal = new TSessionPuttyTelnet();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case ESessionType.PuttySerial: {
            ISession RetVal = new TSessionPuttySerial();
            RetVal.SetLogger(logger);
            return RetVal;
          }

        case ESessionType.OpenSSH: {
            ISession RetVal = new TSessionOpenSsh();
            RetVal.SetLogger(logger);
            return RetVal;
          }
        case ESessionType.Pscp:
        case ESessionType.Scp:
        case ESessionType.CmdPrompt:
        case ESessionType.Powershell:
        case ESessionType.Custom:
          logger.LogWarning($"Session type is not yet implemented : {sessionType}");
          return null;

        case ESessionType.Unknown:
        default:
          return null;
      }
    }

    public virtual ISession Duplicate() {
      ISession RetVal = BuildSession(SessionType, Logger);
      RetVal.Name = Name;
      RetVal.Description = Description;
      RetVal.Comment = Comment;
      RetVal.StorageLocation = StorageLocation;
      RetVal.Parent = Parent;
      RetVal.SetLocalCredential(Credential);
      return RetVal;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Event handlers --------------------------------------------
    public event EventHandler OnStart;
    public event EventHandler OnExit;
    public event EventHandler OnStarted;
    public event EventHandler OnExited;
    #endregion --- Event handlers --------------------------------------------

    #region --- Execution process --------------------------------------------
    /// <summary>
    /// The process to be executed
    /// </summary>
    public TRunProcess RunProcess { get; protected set; } = new();

    /// <summary>
    /// Is the underlying process running
    /// </summary>
    public bool IsRunning => RunProcess.IsRunning;

    /// <summary>
    /// The PID of the underlying process
    /// </summary>
    public int PID => RunProcess.PID;

    /// <summary>
    /// The command line info used to start the underlying process
    /// </summary>
    public string CommandLine => RunProcess?.GetCommandLine();
    #endregion --- Execution process --------------------------------------------

    /// <summary>
    /// Start the underlying process with a list of arguments
    /// </summary>
    /// <param name="arguments">The list of arguments</param>
    public virtual void Start(IEnumerable<string> arguments) {
      Start(string.Join(" ", arguments));
    }

    /// <summary>
    /// Start the underlying process with the arguments in command line form
    /// </summary>
    /// <param name="arguments">The string containing all the arguments well formatted</param>
    public virtual void Start(string arguments = "") {
      OnStart?.Invoke(this, EventArgs.Empty);
      RunProcess.Start();
      OnStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Stop the underlying process
    /// </summary>
    public virtual void Stop() {
      if (RunProcess is null || !IsRunning) {
        return;
      }
      OnExit?.Invoke(this, EventArgs.Empty);
      RunProcess.Cancel();
      OnExited?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Change the title of the underlying process window container
    /// </summary>
    /// <param name="title">The new title</param>
    protected void SetProcessTitle(string title = "") {
      if (RunProcess is not null) {
        RunProcess.ProcessTitle = title;
      }
    }



  }
}
