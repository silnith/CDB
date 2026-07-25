using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.ModelComponents.Component;

public class Element
{
    [XmlElement("Description")]
    public string Description
    {
        get;
        set;
    }

    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }
}
