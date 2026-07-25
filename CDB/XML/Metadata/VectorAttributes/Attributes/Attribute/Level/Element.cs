using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes.Attribute.Level;

[XmlType("Level", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Instance")]
    public LevelPresence? Instance
    {
        get;
        set;
    }

    [XmlElement("Class")]
    public LevelPresence? Class
    {
        get;
        set;
    }

    [XmlElement("Extended")]
    public LevelPresence? Extended
    {
        get;
        set;
    }
}
