using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.FeatureDataDictionary.Category.Subcategory.FeatureType.Subcode;

/// <summary>
/// This element represents a Subcode. It has a code attribute, a label, a concept definition, a recommended dataset component and an origin.
/// </summary>
[XmlType("Subcode", TypeName = "Subcode", Namespace = "http://www.opengis.net/cdb/1.2/Feature_Data_Dictionary")]
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

    [XmlElement("Concept_Definition")]
    public string ConceptDefinition
    {
        get;
        set;
    }

    [XmlElement("Recommended_Dataset_Component")]
    public List<string> RecommendedDatasetComponents
    {
        get;
        set;
    }

    [XmlElement("Origin")]
    public string Origin
    {
        get;
        set;
    }

    [XmlElement("Priority")]
    public int Priority
    {
        get;
        set;
    } = 50;

    /// <summary>
    /// This attribute represents the feature code and subcode.
    /// </summary>
    [XmlAttribute("code")]
    [RegularExpression("[0-9][0-9][0-9]")]
    [Range(0, 999)]
    public int Code
    {
        get;
        set;
    }
}
