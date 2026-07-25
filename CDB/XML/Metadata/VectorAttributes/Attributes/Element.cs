using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Attributes;

[XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Attribute")]
    [MinLength(1)]
    public List<Attribute.Element> Attributes
    {
        get;
        set;
    }
}
