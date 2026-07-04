using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Serialization;

namespace Silnith.CDB.XML;

public class CDBInformation
{
    public Metadata.Version.Element? Version
    {
        get;
        private set;
    }

    public Metadata.Datasets.Element? Datasets
    {
        get;
        set;
    }

    public IReadOnlyDictionary<int, string> DatasetNames
    {
        get;
        private set;
    } = new SortedDictionary<int, string>().ToImmutableSortedDictionary();

    public void Initialize(ICDB dataStore)
    {
        XmlSerializerFactory xmlSerializerFactory = new();
        XmlSerializer versionDeserializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Version.Element));
        XmlSerializer datasetsDeserializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Datasets.Element));

        dataStore.TryReadFile("Metadata/Version.xml", stream =>
        {
            Version = (Metadata.Version.Element?) versionDeserializer.Deserialize(stream);
        });
        dataStore.TryReadFile("Metadata/Datasets.xml", stream =>
        {
            Datasets = (Metadata.Datasets.Element?) datasetsDeserializer.Deserialize(stream);
            DatasetNames = Datasets!.Datasets
                .Select(d => new KeyValuePair<int, string>(d.Code, d.Name))
                .ToImmutableSortedDictionary();
        });

    }
}
