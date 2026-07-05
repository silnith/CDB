using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Lights.Light;

/// <summary>
/// A light has a type and a code.
/// A light can optionally have a description and a list of child lights.
/// </summary>
[XmlType("Light", TypeName = "Light", Namespace = "http://www.opengis.net/cdb/1.2/Lights")]
public class Element
{
    [XmlElement("Description")]
    public string? Description
    {
        get;
        set;
    }

    [XmlElement("Light")]
    public List<Element> ChildLights
    {
        get;
        set;
    }

    [XmlAttribute("type")]
    public string Type
    {
        get;
        set;
    }

    [XmlAttribute("code")]
    [Range(0, 9999)]
    public int Code
    {
        get;
        set;
    }
}
