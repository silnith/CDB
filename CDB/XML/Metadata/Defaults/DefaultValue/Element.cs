using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Defaults.DefaultValue;

public class Element
{
    [XmlElement("Dataset")]
    public string? Dataset
    {
        get;
        set;
    }

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

    [XmlElement("Type")]
    public string Type
    {
        get;
        set;
    }

    [XmlElement("Value")]
    public string Value
    {
        get;
        set;
    }

    [XmlElement("R_W_Type")]
    public string RWType
    {
        get;
        set;
    }
}
