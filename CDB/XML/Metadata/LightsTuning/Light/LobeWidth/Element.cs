using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.LightsTuning.Light.LobeWidth;

/// <summary>
/// Represents the identifying section for the light’s lobe width characteristics, which can have a horizontal and vertical attribute.
/// </summary>
[XmlType("Lobe_Width", Namespace = "http://www.opengis.net/cdb/1.2/Lights_Tuning")]
public class Element
{
    /// <summary>
    /// When a light type is non-native to the CDB specification, the Horizontal field represents the light point’s half-intensity horizontal lobe width for the client-device (range from 0.0 to 360.0). When the light entry is native to the CDB standard, Horizontal field is used as a floating-point modifier that multiplies the horizontal lobe width calculated by the client-device. Applies only to Directional and Bidirectional light types. If absent in a light type entry, Horizontal field defaults to a value of 1.0.
    /// </summary>
    [XmlElement("Horizontal")]
    public double Horizontal
    {
        get;
        set;
    } = 1.0;

    /// <summary>
    /// When a light type is non-native to the CDB standard, Vertical field represents the light point’s half-intensity vertical lobe width for the client-device (range from 0.0 to 360.0). When the light entry is native to the CDB standard, Vertical field is used as a floating-point modifier that multiplies the vertical lobe width calculated by the client-device. This applies only to Directional and Bidirectional light types. If absent in a light type entry, Vertical field defaults to a value of 1.0.
    /// </summary>
    [XmlElement("Vertical")]
    public double Vertical
    {
        get;
        set;
    } = 1.0;
}
