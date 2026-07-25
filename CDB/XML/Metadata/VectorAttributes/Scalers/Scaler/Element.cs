using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Scalers.Scaler;

[XmlType("Scaler", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Name")]
    public string Name
    {
        get;
        set;
    }

    [XmlElement("Description")]
    public string Description
    {
        get;
        set;
    }

    [XmlElement("Multiplier")]
    [Range(0.0, double.MaxValue)]
    public double Multiplier
    {
        get;
        set;
    }

    [XmlAttribute("code")]
    public int Code
    {
        get;
        set;
    }

    [XmlAttribute("symbol")]
    public string Symbol
    {
        get;
        set;
    }
}
