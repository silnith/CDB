using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Configuration.Version.Extension;

/// <summary>
/// Indicates that the CDB Version contains extensions to the CDB Specification.
/// The CDB Extension is identified by a name and a version number.
/// Both are character strings of at least one character.
/// </summary>
[XmlType("Extension", Namespace = "http://www.opengis.net/cdb/1.2/Configuration")]
public class Element
{
    [XmlAttribute("name")]
    [MinLength(1)]
    public string Name
    {
        get;
        set;
    }

    [XmlAttribute("version")]
    [MinLength(1)]
    public string Version
    {
        get;
        set;
    }
}
