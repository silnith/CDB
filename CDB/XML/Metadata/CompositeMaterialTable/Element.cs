using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.CompositeMaterialTable;

/// <summary>
/// A CMT is a list of one or more composite materials.
/// </summary>
[XmlRoot("Composite_Material_Table", Namespace = "http://www.opengis.net/cdb/1.2/Composite_Material_Table")]
public class Element
{
    /// <summary>
    /// Each composite material has a unique identification number, a name, and one or more substrates.
    /// </summary>
    [XmlElement("Composite_Material")]
    [MinLength(1)]
    public List<CompositeMaterial.Element> CompositeMaterials
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
