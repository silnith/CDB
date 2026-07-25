using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.CompositeMaterialTable.CompositeMaterial.Substrate.Material;

/// <summary>
/// Each material is identified by the name of its base material and by its proportion in the substrate.
/// </summary>
[XmlType("Material", Namespace = "http://www.opengis.net/cdb/1.2/Composite_Material_Table")]
public class Element
{
    [XmlElement("Name")]
    public string Name
    {
        get;
        set;
    }

    [XmlElement("Weight")]
    [Range(1, 100)]
    public int Weight
    {
        get;
        set;
    } = 100;
}
