using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.CompositeMaterialTable.CompositeMaterial.Substrate;

/// <summary>
/// A substrate has a certain thickness and is composed of one or more base materials.
/// </summary>
[XmlType("Substrate", Namespace = "http://www.opengis.net/cdb/1.2/Composite_Material_Table")]
public class Element
{
    [XmlElement("Material")]
    [MinLength(1)]
    public List<Material.Element> Materials
    {
        get;
        set;
    }

    /// <summary>
    /// The thickness is expressed in meters, with a value greater than zero.
    /// It is optional for the last substrate when several substrates are defined.
    /// Note that the thickness is always optional for the surface substrate.
    /// </summary>
    [XmlElement("Thickness")]
    [Range(0.0, double.MaxValue)]
    public double? Thickness
    {
        get;
        set;
    }
}
