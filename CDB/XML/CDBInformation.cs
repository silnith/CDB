using System.Xml.Serialization;

namespace Silnith.CDB.XML;

public class CDBInformation
{
    public void Initialize(ICDB dataStore)
    {
        XmlSerializerFactory xmlSerializerFactory = new();

        dataStore.TryReadFile("Metadata/CDB_Attributes.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.VectorAttributes.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.VectorAttributes.Element;
        });
        dataStore.TryReadFile("Metadata/Configuration.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Configuration.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.Configuration.Element;
        });
        dataStore.TryReadFile("Metadata/Datasets.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Datasets.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.Datasets.Element;
        });
        dataStore.TryReadFile("Metadata/Defaults.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Defaults.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.Defaults.Element;
        });
        dataStore.TryReadFile("Metadata/DIS_Country_Codes.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.DISCountryCodes.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.DISCountryCodes.Element;
        });
        dataStore.TryReadFile("Metadata/Feature_Data_Dictionary.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.FeatureDataDictionary.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.FeatureDataDictionary.Element;
        });
        dataStore.TryReadFile("Metadata/Geomatics_Attributes.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.VectorAttributes.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.VectorAttributes.Element;
        });
        dataStore.TryReadFile("Metadata/Lights.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Lights.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.Lights.Element;
        });
        //dataStore.TryReadFile("Metadata/Lights_xxx.xml", stream =>
        //{
        //    XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.LightsTuning.Element));
        //    _ = xmlSerializer.Deserialize(stream) as Metadata.LightsTuning.Element;
        //});
        dataStore.TryReadFile("Metadata/Materials.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.BaseMaterialTable.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.BaseMaterialTable.Element;
        });
        dataStore.TryReadFile("Metadata/Model_Components.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.ModelComponents.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.ModelComponents.Element;
        });
        dataStore.TryReadFile("Metadata/Moving_Model_Codes.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.MovingModelCodes.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.MovingModelCodes.Element;
        });
        dataStore.TryReadFile("Metadata/Vendor_Attributes.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.VectorAttributes.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.VectorAttributes.Element;
        });
        dataStore.TryReadFile("Metadata/Version.xml", stream =>
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Version.Element));
            _ = xmlSerializer.Deserialize(stream) as Metadata.Version.Element;
        });

    }
}
