using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.MovingModelCodes.Kind.Domain;

[XmlType("Domain", Namespace = "http://www.opengis.net/cdb/1.2/Moving_Model_Codes")]
public class Element
{
    [XmlElement("Category")]
    [MinLength(0)]
    [MaxLength(256)]
    public List<Category.Element> Category
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

    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }
}
