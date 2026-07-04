using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.FeatureDataDictionary;

/// <summary>
/// This element represents the CDB Feature Data Dictionary root element.
/// It has a version number and the list of all the categories.
/// </summary>
[XmlRoot("Feature_Data_Dictionary", Namespace = "http://www.opengis.net/cdb/1.2/Feature_Data_Dictionary")]
public class Element
{
    /// <summary>
    /// This element represents a Category. It has a code attribute, a label and a list of subcategories.
    /// </summary>
    [XmlElement("Category")]
    public List<Category.Element> Categories
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
