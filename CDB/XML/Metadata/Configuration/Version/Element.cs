using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Configuration.Version;

/// <summary>
/// A CDB Version points to the folder where the data resides.
/// An optional comment can be used to describe the version.
/// It is possible to indicate to which version of the CDB Specification the CDB Version complies.
/// Finally, the CDB Version can indicate if it contains extensions to the Specification.
/// </summary>
[XmlType("Version", Namespace = "http://www.opengis.net/cdb/1.2/Configuration")]
public class Element
{
    /// <summary>
    /// Provides a non-empty path to a folder.
    /// A relative path is prefered although an absolute path is supported.
    /// </summary>
    [XmlElement("Folder")]
    public Folder.Element Folder
    {
        get;
        set;
    }

    [XmlElement("Comment")]
    public string? Comment
    {
        get;
        set;
    }

    /// <summary>
    /// Specifies the version of the CDB Specification used to generate the current CDB Version.
    /// If &apos;Specification&apos; is omitted, the version number is deemed to be 3.0.
    /// </summary>
    [XmlElement("Specification")]
    public Specification.Element? Specification
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates that the CDB Version contains extensions to the CDB Specification.
    /// The CDB Extension is identified by a name and a version number.
    /// Both are character strings of at least one character.
    /// </summary>
    [XmlElement("Extension")]
    public Extension.Element? Extension
    {
        get;
        set;
    }
}
