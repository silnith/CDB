using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.BaseMaterialTable.Version;

[XmlType("Version", Namespace = "http://www.opengis.net/cdb/1.2/Base_Material_Table")]
[Obsolete("Use the attribute.")]
public class Element
{
    [XmlElement("Major")]
    [Range(1, int.MaxValue)]
    public int Major
    {
        get;
        set;
    }

    [XmlElement("Minor")]
    [Range(0, int.MaxValue)]
    public int Minor
    {
        get;
        set;
    }
}
