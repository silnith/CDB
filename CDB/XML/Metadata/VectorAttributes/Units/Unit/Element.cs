using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Units.Unit;

[XmlType("Unit", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
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
