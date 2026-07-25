using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.VectorAttributes;

/// <summary>
/// Attributes are defined through 3 lists: 1) the attributes themselves, 2) their units, and 3) their scalers.
/// </summary>
[XmlRoot("Vector_Attributes", Namespace = "http://www.opengis.net/cdb/1.2/Vector_Attributes")]
public class Element
{
    [XmlElement("Attributes")]
    public Attributes.Element Attributes
    {
        get;
        set;
    }

    [XmlElement("Units")]
    public Units.Element Units
    {
        get;
        set;
    }

    [XmlElement("Scalers")]
    public Scalers.Element Scalers
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
