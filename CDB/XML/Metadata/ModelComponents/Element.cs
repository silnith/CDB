using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.ModelComponents;

/// <summary>
/// The CDB standard provides the means to unambiguously tag any portions of a 3D model (moving model or cultural feature with a modeled representation) with a descriptive name. Component model names are stored in the model components definition file, “\CDB\Metadata\Model_Components.xml” as described in Section 3.1.1, Metadata Directories. The XML file containing the CDB Model Components is part of the CDB standard distribution package. The XML schema is provided in \CDB\Metadata\Schema\Model_Components.xsd delivered with the standard.
/// </summary>
/// <seealso href="https://docs.ogc.org/is/15-113r7/15-113r7.html#ModelComponentsDefinitionFile"/>
[XmlRoot("Model_Components", Namespace = "http://www.opengis.net/cdb/1.2/Model_Components")]
public class Element
{
    [XmlElement("Component")]
    [MinLength(1)]
    public List<Component.Element> Components
    {
        get;
        set;
    }

    [XmlAttribute("version")]
    public string? Version
    {
        get;
        set;
    }
}
