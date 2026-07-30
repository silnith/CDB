using System.IO;
using System.Xml.Serialization;

namespace Silnith.CDB.XML;

public class CDBInformation
{
    public void Initialize(ICDB dataStore)
    {
        XmlSerializerFactory xmlSerializerFactory = new();

        {
            using Stream? stream = new Silnith.CDB.Metadata("CDB_Attributes", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.VectorAttributes.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.VectorAttributes.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Configuration", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Configuration.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.Configuration.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Datasets", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Datasets.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.Datasets.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Defaults", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Defaults.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.Defaults.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("DIS_Country_Codes", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.DISCountryCodes.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.DISCountryCodes.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Feature_Data_Dictionary", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.FeatureDataDictionary.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.FeatureDataDictionary.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Geomatics_Attributes", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.VectorAttributes.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.VectorAttributes.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Lights", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Lights.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.Lights.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Lights_xxx", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Lights.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.Lights.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Materials", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.BaseMaterialTable.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.BaseMaterialTable.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Model_Components", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.ModelComponents.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.ModelComponents.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Moving_Model_Codes", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.MovingModelCodes.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.MovingModelCodes.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Vendor_Attributes", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.VectorAttributes.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.VectorAttributes.Element;
            }
        }
        {
            using Stream? stream = new Silnith.CDB.Metadata("Version", "xml").ReadFromCDB(dataStore);
            if (stream is not null)
            {
                XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Version.Element));
                _ = xmlSerializer.Deserialize(stream) as Metadata.Version.Element;
            }
        }
    }
}
