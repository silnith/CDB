using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes.Scalers;

[XmlType(AnonymousType = true, Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Scaler")]
    [MinLength(1)]
    public List<Scaler.Element> Scalers
    {
        get;
        set;
    }
}
