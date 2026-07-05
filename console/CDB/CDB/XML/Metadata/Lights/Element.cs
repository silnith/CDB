using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Lights;

[XmlRoot("Lights", Namespace = "http://www.opengis.net/cdb/1.2/Lights")]
public class Element
{
    [XmlElement("Light")]
    public List<Light.Element> Lights
    {
        get;
        set;
    }

    /// <summary>
    /// This attribute represents the version number of this file. It has two components: major.minor.
    /// </summary>
    [XmlAttribute("version")]
    [RegularExpression("([1-9]([0-9])*[.]([0-9])+)")]
    public string Version
    {
        get;
        set;
    }
}
