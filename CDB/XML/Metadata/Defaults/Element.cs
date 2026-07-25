using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Defaults;

/// <summary>
/// Default values for all datasets can be stored in the default values metadata file “\CDB\Metadata\Defaults.xml” as described in Section 3.1.1, Metadata Directories. Default values defined throughout the CDB standard are listed in Annex S OGC CDB Core: Model and Physical Structure: Informative Annexes. The XML schema is provided in \CDB\Metadata\Schema\Defaults.xsd delivered with the standard. There are two types of default values: read and write default values (‘R’ or ‘W’.) Generally, read default values are values to be used when optional information is not available. Write default values are default values to be used by CDB creation tools to fill mandatory content when information is either missing or not available. The default value name is a unique name identifying a default value for a given dataset. Valid default value names are listed in Annex S. Each default value has a type. Valid default value data types are “float”, “integer” and “string.”
/// </summary>
/// <seealso href="https://docs.ogc.org/is/15-113r7/15-113r7.html#DefaultValuesDefinitionTable"/>
[XmlRoot("Default_Value_Table", Namespace = "http://www.opengis.net/cdb/1.2/Defaults")]
public class Element
{
    [XmlElement("Default_Value")]
    [MinLength(1)]
    public List<DefaultValue.Element> DefaultValues
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
