using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute.Value;

[XmlType("Value", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Type")]
    public TypeType Type
    {
        get;
        set;
    }

    [XmlElement("Format")]
    public FormatType? Format
    {
        get;
        set;
    }

    [XmlElement("Precision")]
    [RegularExpression("([0-9])+.([0-9])+")]
    public string? Precision
    {
        get;
        set;
    }

    [XmlElement("Range")]
    public Range.Element? Range
    {
        get;
        set;
    }

    [XmlElement("Length")]
    public int? Length
    {
        get;
        set;
    }

    [XmlElement("Unit")]
    public int? Unit
    {
        get;
        set;
    }

    [XmlElement("Scaler")]
    public int? Scaler
    {
        get;
        set;
    }
}
