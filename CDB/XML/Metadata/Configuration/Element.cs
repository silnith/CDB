using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.Configuration;

/// <summary>
/// The CDB Configuration is a simple list of one or more CDB Versions.
/// </summary>
/// <remarks>
/// <para>
/// The concept of a &apos;CDB Configuration&apos; is new to version 3.2 of the Specification.
/// A single XML file, named Configuration.xml, completely defines the configuration of one &apos;logical&apos; CDB.
/// This way, the client application does not have to traverse the linked list of CDB Versions through the &apos;PreviousIncrementalRootDirectory&apos; element found in Version.xml
/// </para>
/// </remarks>
[XmlRoot("Configuration", Namespace = "http://www.opengis.net/cdb/1.2/Configuration")]
public class Element
{
    [XmlElement("Comment")]
    public string? Comment
    {
        get;
        set;
    }

    /// <summary>
    /// A CDB Version points to the folder where the data resides.
    /// An optional comment can be used to describe the version.
    /// It is possible to indicate to which version of the CDB Specification the CDB Version complies.
    /// Finally, the CDB Version can indicate if it contains extensions to the Specification.
    /// </summary>
    [XmlElement("Version")]
    [MinLength(1)]
    public List<Version.Element> Versions
    {
        get;
        set;
    }
}
