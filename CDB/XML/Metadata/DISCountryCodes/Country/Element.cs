using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.DISCountryCodes.Country;

[XmlType("Country", Namespace = "http://www.opengis.net/cdb/1.2/DIS_Country_Codes")]
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

    [XmlAttribute("alpha3code")]
    public string Alpha3Code
    {
        get;
        set;
    }

    [XmlAttribute("validCode")]
    public bool ValidCode
    {
        get;
        set;
    }
}
