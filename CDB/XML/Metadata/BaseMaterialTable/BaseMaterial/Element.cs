using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.BaseMaterialTable.BaseMaterial;

/// <summary>
/// This element defines one CDB Base Material by giving it a unique name.
/// It is recommended to provide a description.
/// </summary>
[XmlType("Base_Material", Namespace = "http://www.opengis.net/cdb/1.2/Base_Material_Table")]
public class Element
{
    [XmlElement("Name")]
    [RegularExpression("[B][M][_]([A-Za-z0-9_-])+")]
    [MaxLength(32)]
    public string Name
    {
        get;
        set;
    }

    [XmlElement("Description")]
    public string? Description
    {
        get;
        set;
    }
}
