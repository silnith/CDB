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

    public Metadata.FeatureDataDictionary.Element? FeatureDataDictionary
    {
        get;
        private set;
    }

    public IReadOnlyDictionary<string, string> FeatureCategoryNames
    {
        get;
        private set;
    } = new SortedDictionary<string, string>().ToImmutableDictionary();

    public IReadOnlyDictionary<string, string> FeatureSubcategoryNames
    {
        get;
        private set;
    } = new SortedDictionary<string, string>().ToImmutableDictionary();

    public IReadOnlyDictionary<FeatureCode, string> FeatureTypeNames
    {
        get;
        private set;
    } = new SortedDictionary<FeatureCode, string>().ToImmutableDictionary();

    public IReadOnlyDictionary<FeatureCode, IEnumerable<int>> ValidFeatureSubcodes
    {
        get;
        private set;
    } = new SortedDictionary<FeatureCode, IEnumerable<int>>().ToImmutableDictionary();

    public void Initialize(ICDB dataStore)
    {
        XmlSerializerFactory xmlSerializerFactory = new();
        XmlSerializer versionSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Version.Element));
        XmlSerializer datasetsSerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.Datasets.Element));
        XmlSerializer featureDataDictionarySerializer = xmlSerializerFactory.CreateSerializer(typeof(Metadata.FeatureDataDictionary.Element));

        dataStore.TryReadFile("Metadata/Version.xml", stream =>
        {
            Version = (Metadata.Version.Element?) versionSerializer.Deserialize(stream);
        });
        dataStore.TryReadFile("Metadata/Datasets.xml", stream =>
        {
            Datasets = (Metadata.Datasets.Element?) datasetsSerializer.Deserialize(stream);
            DatasetNames = Datasets!.Datasets
                .Select(d => new KeyValuePair<int, string>(d.Code, d.Name))
                .ToImmutableSortedDictionary();
        });
        dataStore.TryReadFile("Metadata/Feature_Data_Dictionary.xml", stream =>
        {
            FeatureDataDictionary = (Metadata.FeatureDataDictionary.Element?) featureDataDictionarySerializer.Deserialize(stream);
            FeatureCategoryNames = FeatureDataDictionary!.Categories
                .Select(a => new KeyValuePair<string, string>(a.Code, a.Label))
                .ToImmutableSortedDictionary();
            SortedDictionary<string, IReadOnlyDictionary<string, string>> foo = new();
            foreach (var bar in FeatureDataDictionary!.Categories)
            {
                var category = bar.Code;
                SortedDictionary<string, string> blah = new();
                foreach (var baz in bar.Subcategories)
                {
                    var subcategory = baz.Code;
                    var label = baz.Label;
                    blah.Add(subcategory, label);
                }
                ImmutableSortedDictionary<string, string> immutableSortedDictionary = bar.Subcategories
                    .Select(a => new KeyValuePair<string, string>(a.Code, a.Label))
                    .ToImmutableSortedDictionary();
                foo.Add(category, immutableSortedDictionary);
            }
            SortedDictionary<string, string> categoryNames = new();
            SortedDictionary<string, string> subcategoryNames = new();
            SortedDictionary<FeatureCode, string> featureTypeNames = new();
            SortedDictionary<FeatureCode, IEnumerable<int>> validSubcodes = new();
            foreach (var categoryElement in FeatureDataDictionary!.Categories)
            {
                var category = categoryElement.Code;
                categoryNames.Add(category, categoryElement.Label);
                foreach (var subcategoryElement in categoryElement.Subcategories)
                {
                    var subcategory = subcategoryElement.Code;
                    subcategoryNames.Add(category + subcategory, subcategoryElement.Label);
                    foreach (var featureTypeElement in subcategoryElement.FeatureTypes)
                    {
                        var type = featureTypeElement.Code;
                        FeatureCode featureCode = new(category, subcategory, type);
                        featureTypeNames.Add(featureCode, featureTypeElement.Label);
                        foreach (var subcodeElement in featureTypeElement.Subcodes)
                        {
                            var subcode = subcodeElement.Code;
                            var label4 = subcodeElement.Label;
                        }
                        validSubcodes.Add(featureCode, featureTypeElement.Subcodes.Select(a => a.Code).ToImmutableList());
                    }
                }
            }
            FeatureCategoryNames = categoryNames.ToImmutableSortedDictionary();
            FeatureSubcategoryNames = subcategoryNames.ToImmutableSortedDictionary();
            FeatureTypeNames = featureTypeNames.ToImmutableSortedDictionary();
            ValidFeatureSubcodes = validSubcodes.ToImmutableSortedDictionary();
        });

    }
}
