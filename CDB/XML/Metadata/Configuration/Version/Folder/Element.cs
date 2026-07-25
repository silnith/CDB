using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Configuration.Version.Folder;

/// <summary>
/// Provides a non-empty path to a folder.
/// A relative path is prefered although an absolute path is supported.
/// </summary>
[XmlType("Folder", Namespace = "http://www.opengis.net/cdb/1.2/Configuration")]
public class Element
{
    [XmlAttribute("path")]
    [MinLength(1)]
    public string Path
    {
        get;
        set;
    }
}
