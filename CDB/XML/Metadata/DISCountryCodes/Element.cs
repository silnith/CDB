using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.DISCountryCodes;

[XmlRoot("DIS_Country_Codes", Namespace = "http://www.opengis.net/cdb/1.2/DIS_Country_Codes")]
public class Element
{
    [XmlElement("Country")]
    [MinLength(1)]
    public List<Country.Element> Countries
    {
        get;
        set;
    }

    /// <summary>
    /// This attribute is used to indicate the version of the XML file containing the list of CDB Default Values.
    /// It is independent from the version of the Specification.
    /// </summary>
    [XmlAttribute("version")]
    [RegularExpression("([1-9]([0-9])*[.]([0-9])+)")]
    public string Version
    {
        get;
        set;
    }
}
