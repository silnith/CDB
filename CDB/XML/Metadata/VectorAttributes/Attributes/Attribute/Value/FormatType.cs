using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute.Value;

[XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public enum FormatType
{
    [XmlEnum("Floating-Point")]
    FloatingPoint,
    [XmlEnum("Integer")]
    Integer
}
