using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.CompositeMaterialTable.CompositeMaterial;

/// <summary>
/// Each composite material has a unique identification number, a name, and one or more substrates.
/// </summary>
[XmlType("Composite_Material", Namespace = "http://www.opengis.net/cdb/1.2/Composite_Material_Table")]
public class Element
{
    [XmlElement("Name")]
    public string? Name
    {
        get;
        set;
    }

    /// <summary>
    /// The presence of a surface substrate is optional.
    /// It represents a very thin layer of materials on top of the primary substrate.
    /// </summary>
    [XmlElement("Surface_Substrate")]
    public Substrate.Element? SurfaceSubstrate
    {
        get;
        set;
    }

    [XmlElement("Primary_Substrate")]
    public Substrate.Element PrimarySubstrate
    {
        get;
        set;
    }

    /// <summary>
    /// There can be an unlimited number of secondary substrates underneath the primary substrate.
    /// They are listed in order from top to bottom.
    /// That is, the first secondary substrate appears immediately underneath the primary substrate.
    /// The next secondary substrate is found underneath the first one. And so on.
    /// </summary>
    [XmlElement("Secondary_Substrate")]
    public List<Substrate.Element> SecondarySubstrates
    {
        get;
        set;
    }

    [XmlAttribute("index")]
    [Range(1, int.MaxValue)]
    public int Index
    {
        get;
        set;
    }
}
