using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute.Value.Range;

[XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Min")]
    public double? Min
    {
        get;
        set;
    }

    [XmlElement("Max")]
    public double? Max
    {
        get;
        set;
    }

    [XmlAttribute("interval")]
    public IntervalType? Interval
    {
        get;
        set;
    }
}
