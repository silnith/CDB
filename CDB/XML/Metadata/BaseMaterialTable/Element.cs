using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.BaseMaterialTable;

/// <summary>
/// The Base Material Table is a list of one or more CDB Base Materials.
/// </summary>
/// <seealso href="https://docs.ogc.org/is/15-113r7/15-113r7.html#BaseMaterialsTable"/>
[XmlRoot("Base_Material_Table", Namespace = "http://www.opengis.net/cdb/1.2/Base_Material_Table")]
public class Element
{
    /// <summary>
    /// This element is deprecated and should no longer be used.
    /// It has been replaced by the &apos;version&apos; attribute.
    /// </summary>
    [XmlElement("Version")]
    [Obsolete("Use the attribute.")]
    public Version.Element? DeprecatedVersion
    {
        get;
        set;
    }

    /// <summary>
    /// This element is deprecated and should no longer be used.
    /// </summary>
    [XmlElement("Source")]
    [Obsolete]
    public string? Source
    {
        get;
        set;
    }

    [XmlElement("Base_Material")]
    [MinLength(1)]
    public List<BaseMaterial.Element> BaseMaterial
    {
        get;
        set;
    }

    /// <summary>
    /// This attribute is used to indicate the version of the XML file containing the list of CDB Materials.
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
