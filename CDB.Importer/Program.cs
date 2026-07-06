using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silnith.CDB.SQL;
using Silnith.CDB.SQLite;
using Silnith.CDB.Visitor;
using Silnith.CDB.XML;
using System;
using System.Globalization;
using System.IO;

namespace Silnith.CDB.Importer;

internal class Program
{
    private static IHost Setup(string[] args)
    {
        HostApplicationBuilder hostApplicationBuilder = Host.CreateApplicationBuilder(args);

        hostApplicationBuilder.Services.AddSingleton<DISEntityDirectoryWalker>();
        hostApplicationBuilder.Services.AddSingleton<FeatureCodeDirectoryWalker>();
        hostApplicationBuilder.Services.AddSingleton<LevelOfDetailDirectoryWalker>();
        hostApplicationBuilder.Services.AddSingleton<TextureDirectoryVisitor>();
        hostApplicationBuilder.Services.AddSingleton<TiledDatasetVisitor>();

        hostApplicationBuilder.Services.AddSingleton<MetadataVisitor>();
        hostApplicationBuilder.Services.AddSingleton<GeotypicalModelVisitor>();
        hostApplicationBuilder.Services.AddSingleton<MovingModelVisitor>();
        hostApplicationBuilder.Services.AddSingleton<TiledDatasetVisitor>();
        hostApplicationBuilder.Services.AddSingleton<NavigationVisitor>();

        return hostApplicationBuilder.Build();
    }

    static void Main(string[] args)
    {
        using var host = Setup(args);

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            //DataSource = "CDB.db",
            DataSource = ":memory:",
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default,
            ForeignKeys = true,
            RecursiveTriggers = true,
            Pooling = true,
        };
        using SqliteConnection sqliteConnection = new(connectionStringBuilder.ConnectionString);
        sqliteConnection.Open();
        using SQLiteDataStore sqliteDataStore = new(sqliteConnection, true);

        using (StreamWriter streamWriter = File.CreateText("schema.txt"))
        {
            sqliteDataStore.DumpStatements(streamWriter);
        }

        string cdbName = "CDB";
        DirectoryInfo cdbRoot = new(cdbName);

        DateTimeOffset start = DateTimeOffset.UtcNow;

        sqliteDataStore.ImportDirectory(cdbName, cdbRoot, host.Services);

        DateTimeOffset end = DateTimeOffset.UtcNow;

        Console.WriteLine("Import time: {0}", end - start);

        using ICDB cdb = new SQLCDB(cdbName, cdbRoot, sqliteDataStore);
        CDBInformation cdbInformation = new();
        cdbInformation.Initialize(cdb);

        foreach ((int code, string name) in cdbInformation.DatasetNames)
        {
            //Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:D3}_{1}", code, name));
        }
        foreach (FeatureCode featureCode in cdbInformation.ValidFeatureSubcodes.Keys)
        {
            //Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} = {1}, {2}, {3}",
            //    featureCode.Code,
            //    cdbInformation.FeatureCategoryNames[featureCode.Category],
            //    cdbInformation.FeatureSubcategoryNames[featureCode.Category + featureCode.Subcategory],
            //    cdbInformation.FeatureTypeNames[featureCode]));
        }

        start = DateTimeOffset.UtcNow;

        int fileCount = 0;
        sqliteDataStore.WalkDirectory(cdbRoot, host.Services, (obj, fileInfo) =>
        {
            string relativePath = Path.GetRelativePath(cdb.CdbRoot.FullName, fileInfo.FullName);
            cdb.TryReadFile(relativePath, stream =>
            {
                fileCount++;
                //Console.WriteLine("Found {0}", relativePath);
            });
        });

        end = DateTimeOffset.UtcNow;

        Console.WriteLine("Walk time: {0}", end - start);
    }
}
