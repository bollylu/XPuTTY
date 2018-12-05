using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EasyPutty.Interfaces;

namespace EasyPutty.Models {
  public class TConfig : IName {

    #region XML constants
    public static XName XML_THIS_ELEMENT {
      get {
        return GetXName("Root");
      }
    }

    public const string XML_ELEMENT_CONFIG = "config";
    public const string XML_ATTRIBUTE_NAME = "Name";
    public const string XML_ATTRIBUTE_DESCRIPTION = "Description";
    public const string XML_ATTRIBUTE_COMMENT = "Comment";

    public static string DefaultXmlNamespace {
      get {
        return "http://easyputty.sharenet.be";
      }
    }

    public static string XmlNamespace {
      get {
        if ( _XmlNamespace == null ) {
          return DefaultXmlNamespace;
        } else {
          return _XmlNamespace;
        }
      }
      set {
        _XmlNamespace = value;
      }
    }
    protected static string _XmlNamespace;

    public static XName GetXName(string name = "") {
      if ( name == "" ) {
        return XName.Get(XmlNamespace);
      } else {
        return XName.Get(name, XmlNamespace);
      }
    }

    public static XName GetXName(string name, string xmlNamespace) {
      if ( name == "" ) {
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

    /// <summary>
    /// The location where to save or load data
    /// </summary>
    public string StorageLocation { get; set; }

    #region Constructor(s)
    public TConfig() {
      _Initialize();
    }
    public TConfig(string storageLocation) : base() {
      StorageLocation = storageLocation;
      _Initialize();
    }
    public TConfig(TConfig config) : base() {
      _Initialize();
      #region Validate parameters
      if ( config == null ) {
        return;
      }
      #endregion Validate parameters
      StorageLocation = config.StorageLocation;
    }

    protected void _Initialize() {
      if ( StorageLocation == null ) {
        StorageLocation = "";
      }
    }
    #endregion Constructor(s)


  }
}
