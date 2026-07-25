using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute;

[XmlType("Attribute", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Name")]
    [MinLength(1)]
    public string Name
    {
        get;
        set;
    }

    [XmlElement("Description")]
    [MinLength(1)]
    public string Description
    {
        get;
        set;
    }

    [XmlElement("Level")]
    public Level.Element Level
    {
        get;
        set;
    }

    [XmlElement("Value")]
    public Value.Element Value
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

    [XmlAttribute("deprecated")]
    public bool Deprecated
    {
        get;
        set;
    } = false;
}
