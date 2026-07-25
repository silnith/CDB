using System.Collections.Generic;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.LightsTuning;

[XmlRoot("Lights_Tuning", Namespace = "http://www.opengis.net/cdb/1.2/Lights_Tuning")]
public class Element
{
    [XmlElement("Light")]
    public List<Light.Element> Lights
    {
        get;
        set;
    }

    [XmlAttribute("version")]
    public string? Version
    {
        get;
        set;
    }
}
