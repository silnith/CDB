using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute.Level;

[XmlType("Level_Presence", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public enum LevelPresence
{
    [XmlEnum("Preferred")]
    Preferred,
    [XmlEnum("Deprecated")]
    Deprecated,
    [XmlEnum("Supported")]
    Supported,
    [XmlEnum("Not Supported")]
    NotSupported
}
