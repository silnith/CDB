using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.MovingModelCodes.Kind.Domain.Category;

[XmlType("Category", Namespace = "http://www.opengis.net/cdb/1.2/Moving_Model_Codes")]
public class Element
{
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
