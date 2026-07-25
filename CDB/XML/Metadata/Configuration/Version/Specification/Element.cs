using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Configuration.Version.Specification;

/// <summary>
/// Specifies the version of the CDB Specification used to generate the current CDB Version.
/// If &apos;Specification&apos; is omitted, the version number is deemed to be 3.0.
/// </summary>
[XmlType("Specification", Namespace = "http://www.opengis.net/cdb/1.2/Configuration")]
public class Element
{
    [XmlAttribute("version")]
    public string Version
    {
        get;
        set;
    }
}
