using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Silnith.CDB.XML.Metadata.LightsTuning.Light;

/// <summary>
/// Client-devices use the light type code as an index to lookup the client-specific properties and characteristics of each light type. This approach is client-device independent because the (device-specific) client’s rendering parameters are local to its implementation. As a result, modelers need not bother setting or even understanding the many parameters specific to each light type and to each client-device type.
/// </summary>
/// <seealso href="https://docs.ogc.org/is/15-113r7/15-113r7.html#ClientSpecificLightsDefinitionMetadata"/>
[XmlType("Light", Namespace = "http://www.opengis.net/cdb/1.2/Lights_Tuning")]
public class Element
{
    [XmlElement("Description")]
    public string? Description
    {
        get;
        set;
    }

    /// <summary>
    /// When a light type is non-native to the CDB standard, which means that it is without a corresponding entry in Annex J Intensity represents the light point intensity for the client-device (range normalized from 0.0 to 1.0). When the light entry is native to the CDB standard, Intensity is used as a floating-point intensity modifier that multiplies the intensity calculated by the client-device. In both cases, Intensity defaults to a value of 1.0.
    /// </summary>
    [XmlElement("Intensity")]
    public double Intensity
    {
        get;
        set;
    } = 1.0;

    /// <summary>
    /// When a light type is non-native to the CDB standard, Color is a floating-point RGB triplet that represents the color of the light type for the client-device (range normalized from 0.0 to 1.0). When the light entry is native to the CDB specification, Color is a floating-point RGB triplet that multiplies the RGB value calculated by the client-device. Color applies only to visual system client-device types. If absent in a light type entry, Color defaults to a value of white (1.0, 1.0, 1.0).
    /// </summary>
    [XmlElement("Color")]
    [MinLength(3)]
    [MaxLength(3)]
    [Range(0, double.MaxValue)]
    public List<double> Color
    {
        get;
        set;
    } = new List<double> { 1.0, 1.0, 1.0, };

    /// <summary>
    /// A string that categorizes the light type as “Omnidirectional”, “Directional” or “Bidirectional”. If absent in a light type entry, Directionality defaults to the value “Omnidirectional.”
    /// </summary>
    [XmlElement("Directionality")]
    public DirectionalityType Directionality
    {
        get;
        set;
    } = DirectionalityType.Omnidirectional;

    /// <summary>
    /// A floating-point value greater than or equal to 0.0 that sets the blink or rotating frequency of the light in Hertz (cycles per second). A value of 0.0 disables all blinking and rotating properties. If absent in a light type entry, Frequency defaults to a value of 0.0.
    /// </summary>
    [XmlElement("Frequency")]
    public double Frequency
    {
        get;
        set;
    } = 0.0;

    /// <summary>
    /// A floating-point value ranging from 0.0 to 1.0 that sets the duty cycle of the light. Duty cycle is defined as the percentage of time the light is turned on over a complete cycle. A value of 0.0 permanently turns the light off. A value of 1.0 turns it on. The value is ignored if Frequency = 0.0. If absent in a light type entry, Duty_Cycle defaults to a value of 0.5.
    /// </summary>
    [XmlElement("Duty_Cycle")]
    public double DutyCycle
    {
        get;
        set;
    } = 0.5;

    /// <summary>
    /// When a light type is non-native to the CDB standard, Residual_Intensity represents the residual intensity of the light. Residual intensity is the intensity of the light (range normalized from 0.0 to 1.0) outside of the lobe defined by Lobe_Width:Horizontal and Lobe_Width:Vertical fields. When the light entry is native to the CDB specification, Residual_Intensity is used as a floating-point modifier that multiplies the residual intensity calculated by the client-device. This applies only to Directional and Bidirectional light types. If absent in a light type entry, Residual_Intensity defaults to a value of 1.0.
    /// </summary>
    [XmlElement("Residual_Intensity")]
    public double ResidualIntensity
    {
        get;
        set;
    } = 1.0;

    /// <summary>
    /// Represents the identifying section for the light’s lobe width characteristics, which can have a horizontal and vertical attribute.
    /// </summary>
    [XmlElement("Lobe_Width")]
    public LobeWidth.Element? LobeWidth
    {
        get;
        set;
    }

    [XmlAttribute("type")]
    public string Type
    {
        get;
        set;
    }
}

public enum DirectionalityType
{
    [XmlEnum("Omnidirectional")]
    Omnidirectional,
    [XmlEnum("Directional")]
    Directional,
    [XmlEnum("Bidirectional")]
    Bidirectional
}
