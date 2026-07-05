using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.FeatureDataDictionary.Category.Subcategory;

/// <summary>
/// This element represents a Subcategory. It has a code attribute, a label and a list of feature types.
/// </summary>
[XmlType("Subcategory", TypeName = "Subcategory", Namespace = "http://www.opengis.net/cdb/1.2/Feature_Data_Dictionary")]
public class Element
{
    /// <summary>
    /// This attribute represents the Label. It is a meaningful name to the code attribute.
    /// </summary>
    [XmlElement("Label")]
    [RegularExpression("([A-Za-z0-9_-])+")]
    [MaxLength(32)]
    public string Label
    {
        get;
        set;
    }

    /// <summary>
    /// This element represents a Feature_Type. It has a code attribute, a label and a list of subcodes.
    /// </summary>
    [XmlElement("Feature_Type")]
    public List<FeatureType.Element> FeatureTypes
    {
        get;
        set;
    }

    /// <summary>
    /// This attribute represents the code of the category or subcategory.
    /// </summary>
    [XmlAttribute("code")]
    [RegularExpression("[A-Z]")]
    [StringLength(1, MinimumLength = 1)]
    public string Code
    {
        get;
        set;
    }
}
