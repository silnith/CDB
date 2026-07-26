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
/// A distinct type for datasets.
/// </summary>
/// <remarks>
/// <para>
/// Dataset codes are listed in Annex Q of OGC CDB Core: Model and Physical Structure: Informative Annexes.
/// </para>
/// </remarks>
/// <param name="Value">The dataset code.</param>
public record Dataset([property: Range(0, 999)] int Value) : IComparable<Dataset>
{
    /// <summary>
    /// A pattern for datasets as they are used in CDB tiled dataset directories.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader><term>Capture Group</term><description>Meaning</description></listheader>
    /// <item><term>dataset</term><description>Parseable as an integer.</description></item>
    /// <item><term>name</term><description>The name of the dataset.</description></item>
    /// </list>
    /// </remarks>
    public static Regex DirectoryPattern
    {
        get;
    } = new(@"^(?<dataset>\d{3})_(?<name>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    /// <summary>
    /// Returns a dataset object by extracting the capture groups from
    /// a match against <see cref="DirectoryPattern"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This expects capture groups exactly matching
    /// <c>(?&lt;dataset&gt;\d{3})</c> and <c>(?&lt;name&gt;.+)</c>.
    /// </para>
    /// </remarks>
    /// <param name="match">The successful match against <see cref="DirectoryPattern"/>.</param>
    /// <returns>A dataset object.</returns>
    public static Dataset FromDirectoryMatch(Match match)
    {
        return new(int.Parse(match.Groups["dataset"].Value, CultureInfo.InvariantCulture));
    }

    private static IReadOnlyDictionary<int, string> DatasetNames
    {
        get;
    }

    static Dataset()
    {
        const string DatasetsResource = "Metadata/Datasets.xml";
        Assembly assembly = typeof(Dataset).Assembly;

        XmlSerializerFactory xmlSerializerFactory = new();

        object? deserialized;
        using (Stream stream = assembly.GetManifestResourceStream(DatasetsResource)
            ?? throw new ApplicationException($"Resource {DatasetsResource} is missing from assembly {assembly}"))
        {
            XmlSerializer xmlSerializer = xmlSerializerFactory.CreateSerializer(typeof(XML.Metadata.Datasets.Element));
            deserialized = xmlSerializer.Deserialize(stream);
        }

        if (deserialized is XML.Metadata.Datasets.Element datasets)
        {
            static KeyValuePair<int, string> MakeDataset(XML.Metadata.Datasets.Dataset.Element dataset)
            {
                return new KeyValuePair<int, string>(dataset.Code, dataset.Name);
            }

            DatasetNames = datasets.Datasets
                .Select(MakeDataset)
                .ToImmutableSortedDictionary();
        }
        else
        {
            throw new ApplicationException($"Unable to parse resource {DatasetsResource} in assembly {assembly}");
        }
    }

    /// <summary>
    /// The dataset code when used in a filename.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will be of the form <c>D000</c>.
    /// </para>
    /// </remarks>
    public string Code => $"D{Value:D3}";

    /// <summary>
    /// The dataset code when used as a directory name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will be of the form <c>000_Name</c>.
    /// </para>
    /// </remarks>
    public string Directory
    {
        get
        {
            static string DirectoryName(int val)
            {
                return $"{val:D3}_{DatasetNames[val]}";
            }

            return Value switch
            {
                500 or 510 or 503 => DirectoryName(500),
                501 or 511 or 504 or 505 => DirectoryName(501),
                506 or 508 => DirectoryName(506),
                507 or 509 or 513 => DirectoryName(507),
                502 or 512 => DirectoryName(502),
                600 or 603 => DirectoryName(600),
                601 or 604 or 605 => DirectoryName(601),
                606 => DirectoryName(606),
                _ => DirectoryName(Value),
            };
        }
    }

    /// <summary>
    /// The first directory inside of the CDB that contains files with this dataset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will always be one of:
    /// </para>
    /// <list type="bullet">
    /// <item><term>Metadata</term><description>Global metadata</description></item>
    /// <item><term>GTModel</term><description>Geotypical models</description></item>
    /// <item><term>MModel</term><description>Moving models</description></item>
    /// <item><term>Tiles</term><description>Tiled datasets</description></item>
    /// <item><term>Navigation</term><description>Navigation data</description></item>
    /// </list>
    /// </remarks>
    public string RootDirectory
    {
        get
        {
            if (Value == 400)
            {
                return "Navigation";
            }

            return (Value / 100) switch
            {
                0 or 1 or 2 or 3 or 4 => "Tiles",
                5 => "GTModel",
                6 => "MModel",
                7 => "Metadata",
                _ => "Unknown",
            };
        }
    }

    /// <summary>
    /// Whether this dataset lives inside of a zip archive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following datasets are contained inside of zip archives:
    /// <list type="bullet">
    /// <item><term>300</term><description>GSModelGeometry</description></item>
    /// <item><term>301</term><description>GSModelTexture</description></item>
    /// <item><term>302</term><description>GSModelSignature</description></item>
    /// <item><term>303</term><description>GSModelDescriptor</description></item>
    /// <item><term>304</term><description>GSModelMaterial</description></item>
    /// <item><term>305</term><description>GSModelInteriorGeometry</description></item>
    /// <item><term>306</term><description>GSModelInteriorTexture</description></item>
    /// <item><term>307</term><description>GSModelInteriorDescriptor</description></item>
    /// <item><term>308</term><description>GSModelInteriorMaterial</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool IsZipped
    {
        get
        {
            return Value switch
            {
                300 or 301 or 302 or 303 or 304 or 305 or 306 or 307 or 308 => true,
                _ => false,
            };
        }
    }

    /// <inheritdoc/>
    public int CompareTo(Dataset? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Value.CompareTo(other.Value);
    }
}
