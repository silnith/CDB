using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.MovingModelCodes;

[XmlRoot("Moving_Model_Codes", Namespace = "http://www.opengis.net/cdb/1.2/Moving_Model_Codes")]
public class Element
{
    [XmlElement("Kind")]
    [MinLength(1)]
    [MaxLength(256)]
    public List<Kind.Element> Kinds
    {
        get;
        set;
    }

    /// <summary>
    /// This attribute is used to indicate the version of the XML file containing the list of codes.
    /// It is independent from the version of the Specification.
    /// It is also independent from the version of the Schema.
    /// </summary>
    [XmlAttribute("version")]
    [RegularExpression("([1-9]([0-9])*[.]([0-9])+)")]
    public string Version
    {
        get;
        set;
    }
}
