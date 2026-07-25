using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute.Value.Range;

[XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public enum IntervalType
{
    [XmlEnum("Open")]
    Open,
    [XmlEnum("Left-Open")]
    LeftOpen,
    [XmlEnum("Right-Open")]
    RightOpen
}
