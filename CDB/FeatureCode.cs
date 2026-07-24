using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Silnith.CDB;

/// <summary>
/// A feature code (FC) used to classify features in the CDB standard.
/// </summary>
/// <remarks>
/// <para>
/// The feature code is a five-character code where the first character
/// represents a category of features, the second represents a subcategory
/// of the current category, and the last three characters represent a
/// specific type in the subcategory.
/// </para>
/// <para>
/// The full list of feature codes is available in the <c>Feature_Data_Dictionary.xml</c>
/// file in a CDB Metadata directory.
/// </para>
/// <para>
/// This code implements Section 3.3.8.1 Feature Classification from the CDB standard,
/// volume 1.
/// </para>
/// </remarks>
/// <param name="Category">The feature category.</param>
/// <param name="Subcategory">The feature subcategory of the category.</param>
/// <param name="Type">The feature type in the subcategory.</param>
/// <seealso href="https://github.com/opengeospatial/cdb-volume-1"/>
public record FeatureCode(
    [property: MaxLength(1)] string Category,
    [property: MaxLength(1)] string Subcategory,
    [property: Range(0, 999)] int Type) : IComparable<FeatureCode>
{
    /// <summary>
    /// Matches directory names of the form <c>A_Category</c>,
    /// where the <c>A</c> is the first character of a feature code,
    /// and <c>Category</c> is the name of the category identified
    /// by the code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The capture groups of this pattern are <c>category</c> and <c>name</c>.
    /// </para>
    /// </remarks>
    public static Regex CategoryDirectoryPattern
    {
        get;
    } = new(@"^(?<category>[A-Z])_(?<name>.+)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <summary>
    /// Matches directory names of the form <c>B_Subcategory</c>,
    /// where the <c>B</c> is the second character of the feature code,
    /// and <c>Subcategory</c> is the name of the subcategory identified
    /// by the code.  The subcategory is relative to the category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The capture groups of this pattern are <c>subcategory</c> and <c>name</c>.
    /// </para>
    /// </remarks>
    public static Regex SubcategoryDirectoryPattern
    {
        get;
    } = new(@"^(?<subcategory>[A-Z])_(?<name>.+)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <summary>
    /// Matches directory names of the form <c>999_Type</c>,
    /// where the <c>999</c> is the last three characters of the feature code,
    /// and <c>Type</c> is the name of the feature type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The capture groups of this pattern are <c>type</c> and <c>name</c>.
    /// Capture group <c>type</c> can be parsed as a non-negative integer.
    /// </para>
    /// </remarks>
    public static Regex TypeDirectoryPattern
    {
        get;
    } = new(@"^(?<type>\d{3})_(?<name>.+)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <summary>
    /// Returns a feature code by extracting the capture groups from
    /// matches against the directory patterns for feature code.
    /// </summary>
    /// <param name="categoryMatch">A successful match against <see cref="CategoryDirectoryPattern"/>.</param>
    /// <param name="subcategoryMatch">A successful match against <see cref="SubcategoryDirectoryPattern"/>.</param>
    /// <param name="typeMatch">A successful match against <see cref="TypeDirectoryPattern"/>.</param>
    /// <returns>A feature code.</returns>
    public static FeatureCode FromDirectoryPatternMatches(Match categoryMatch, Match subcategoryMatch, Match typeMatch)
    {
        return new(
            categoryMatch.Groups["category"].Value,
            subcategoryMatch.Groups["subcategory"].Value,
            int.Parse(typeMatch.Groups["type"].Value, CultureInfo.InvariantCulture));
    }

    private static IReadOnlyDictionary<string, string> FeatureCategoryNames
    {
        get;
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FeatureSubcategoryNames
    {
        get;
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<int, string>>> FeatureTypeNames
    {
        get;
    }

    static FeatureCode()
    {
        Assembly assembly = typeof(FeatureCode).Assembly;
        const string FeatureDictionaryResource = "Metadata/Feature_Data_Dictionary.xml";

        XmlSerializerFactory xmlSerializerFactory = new();

        object? deserialized;
        using (Stream stream = assembly.GetManifestResourceStream(FeatureDictionaryResource)
            ?? throw new ApplicationException($"Resource {FeatureDictionaryResource} is missing from assembly {assembly}"))
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(XML.Metadata.FeatureDataDictionary.Element));
            deserialized = xmlSerializer.Deserialize(stream);
        }

        if (deserialized is XML.Metadata.FeatureDataDictionary.Element featureData)
        {
            static KeyValuePair<string, string> MakeCategory(XML.Metadata.FeatureDataDictionary.Category.Element category)
            {
                return new KeyValuePair<string, string>(category.Code.ToUpperInvariant(), category.Label);
            }

            FeatureCategoryNames = featureData.Categories
                .Select(MakeCategory)
                .ToImmutableSortedDictionary();

            static KeyValuePair<string, IReadOnlyDictionary<string, string>> MakeSubcategory(XML.Metadata.FeatureDataDictionary.Category.Element category)
            {
                static KeyValuePair<string, string> MakeSubcategory(XML.Metadata.FeatureDataDictionary.Category.Subcategory.Element subcategory)
                {
                    return new KeyValuePair<string, string>(subcategory.Code.ToUpperInvariant(), subcategory.Label);
                }

                return new KeyValuePair<string, IReadOnlyDictionary<string, string>>(
                    category.Code.ToUpperInvariant(),
                    category.Subcategories
                        .Select(MakeSubcategory)
                        .ToImmutableSortedDictionary());
            }

            FeatureSubcategoryNames = featureData.Categories
                .Select(MakeSubcategory)
                .ToImmutableSortedDictionary();

            static KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyDictionary<int, string>>> MakeType(XML.Metadata.FeatureDataDictionary.Category.Element category)
            {
                static KeyValuePair<string, IReadOnlyDictionary<int, string>> MakeType(XML.Metadata.FeatureDataDictionary.Category.Subcategory.Element subcategory)
                {
                    static KeyValuePair<int, string> MakeType(XML.Metadata.FeatureDataDictionary.Category.Subcategory.FeatureType.Element type)
                    {
                        return new KeyValuePair<int, string>(type.Code, type.Label);
                    }

                    return new KeyValuePair<string, IReadOnlyDictionary<int, string>>(
                        subcategory.Code.ToUpperInvariant(),
                        subcategory.FeatureTypes.Select(MakeType).ToImmutableSortedDictionary());
                }

                return new KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyDictionary<int, string>>>(
                    category.Code.ToUpperInvariant(),
                    category.Subcategories.Select(MakeType).ToImmutableSortedDictionary());
            }

            FeatureTypeNames = featureData.Categories
                .Select(MakeType)
                .ToImmutableSortedDictionary();
        }
        else
        {
            throw new ApplicationException($"Unable to parse resource {FeatureDictionaryResource} in assembly {assembly}");
        }
    }

    /// <summary>
    /// The five-character code.
    /// </summary>
    public string Code => $"{Category.ToUpperInvariant()}{Subcategory.ToUpperInvariant()}{Type:D3}";

    public string RelativePath => Path.Combine(
        $"{Category.ToUpperInvariant()}_{FeatureCategoryNames[Category.ToUpperInvariant()]}",
        $"{Subcategory.ToUpperInvariant()}_{FeatureSubcategoryNames[Category.ToUpperInvariant()][Subcategory.ToUpperInvariant()]}",
        $"{Type:D3}_{FeatureTypeNames[Category.ToUpperInvariant()][Subcategory.ToUpperInvariant()][Type]}");

    /// <inheritdoc/>
    public int CompareTo(FeatureCode? other)
    {
        if (other is null)
        {
            return 1;
        }

        int categoryComparison = CultureInfo.InvariantCulture.CompareInfo.Compare(Category, other.Category, CompareOptions.IgnoreCase);
        if (categoryComparison != 0)
        {
            return categoryComparison;
        }

        int subcategoryComparison = CultureInfo.InvariantCulture.CompareInfo.Compare(Subcategory, other.Subcategory, CompareOptions.IgnoreCase);
        if (subcategoryComparison != 0)
        {
            return subcategoryComparison;
        }

        return Type.CompareTo(other.Type);
    }
}
