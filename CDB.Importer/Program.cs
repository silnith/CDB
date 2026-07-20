using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silnith.CDB.FileSystem;
using Silnith.CDB.FileSystem.Visitor;
using Silnith.CDB.SQL;
using Silnith.CDB.SQL.SQLite;
using Silnith.CDB.XML;
using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Silnith.CDB.Importer;

internal class Program
{
    private static IHost Setup(string[] args)
    {
        HostApplicationBuilder hostApplicationBuilder = Host.CreateApplicationBuilder(args);

        hostApplicationBuilder.Services.AddSingleton<DISEntityDirectoryWalker>();
        hostApplicationBuilder.Services.AddSingleton<FeatureCodeDirectoryWalker>();
        hostApplicationBuilder.Services.AddSingleton<LevelOfDetailDirectoryWalker>();
        hostApplicationBuilder.Services.AddSingleton<TextureDirectoryWalker>();

        hostApplicationBuilder.Services.AddSingleton<MetadataVisitor>();
        hostApplicationBuilder.Services.AddSingleton<GeotypicalModelVisitor>();
        hostApplicationBuilder.Services.AddSingleton<MovingModelVisitor>();
        hostApplicationBuilder.Services.AddSingleton<TiledDatasetVisitor>();
        hostApplicationBuilder.Services.AddSingleton<NavigationVisitor>();

        hostApplicationBuilder.Services.AddSingleton<FileSystemCDB>();
        hostApplicationBuilder.Services.AddOptions<FileSystemCDBSettings>()
            .Configure(settings =>
            {
                settings.Root = new("CDB");
            });
        hostApplicationBuilder.Services.AddSingleton<SqliteConnectionStringBuilder>(provider =>
        {
            return new()
            {
                DataSource = "CDB.db",
                //DataSource = ":memory:",
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Default,
                ForeignKeys = true,
                RecursiveTriggers = true,
                Pooling = true,
            };
        });
        hostApplicationBuilder.Services.AddSingleton<DbDataSource, SQLiteDataSource>();
        hostApplicationBuilder.Services.AddSingleton<SQLDataStore, SQLiteDataStore>();
        hostApplicationBuilder.Services.AddOptions<SQLiteDataStoreSettings>()
            .Configure(settings =>
            {
                settings.CreateSchema = true;
            });
        hostApplicationBuilder.Services.AddSingleton<SQLCDB>();
        hostApplicationBuilder.Services.AddOptions<SQLCDBSettings>()
            .Configure(settings =>
            {
                settings.Name = "CDB";
            });

        return hostApplicationBuilder.Build();
    }

    static void Main(string[] args)
    {
        using var host = Setup(args);

        ILogger logger = host.Services.GetRequiredService<ILogger<Program>>();

        SQLDataStore sqlDataStore = host.Services.GetRequiredService<SQLDataStore>();

        using (StreamWriter streamWriter = File.CreateText("schema.txt"))
        {
            sqlDataStore.DumpStatements(streamWriter);
        }

        SQLCDB sqlCDB = host.Services.GetRequiredService<SQLCDB>();
        string cdbName = sqlCDB.Name;
        FileSystemCDB fileSystemCDB = host.Services.GetRequiredService<FileSystemCDB>();

        DateTimeOffset start = DateTimeOffset.UtcNow;

        {
            FileStreamOptions fileStreamOptions = new()
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.Read,
                Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
            };
            void metadataAction(Metadata metadata, FileInfo file)
            {
                logger.LogInformation("Inserting Metadata {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoMetadata(cdbName, metadata, fileStream);
            }
            void geotypicalModelAction(GeotypicalModel geotypicalModel, FileInfo file)
            {
                logger.LogInformation("Inserting Geotypical Model {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoGeotypicalModel(cdbName, geotypicalModel, fileStream);
            }
            void geotypicalModelLodAction(GeotypicalModelLod geotypicalModelLod, FileInfo file)
            {
                logger.LogInformation("Inserting Geotypical Model LOD {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoGeotypicalModelLod(cdbName, geotypicalModelLod, fileStream);
            }
            void textureAction(Texture texture, FileInfo file)
            {
                logger.LogInformation("Inserting Texture {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoTexture(cdbName, texture, fileStream);
            }
            void textureLodAction(TextureLod textureLod, FileInfo file)
            {
                logger.LogInformation("Inserting Texture LOD {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoTextureLod(cdbName, textureLod, fileStream);
            }
            void movingModelAction(MovingModel movingModel, FileInfo file)
            {
                logger.LogInformation("Inserting Moving Model {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoMovingModel(cdbName, movingModel, fileStream);
            }
            void movingModelLodAction(MovingModelLod movingModelLod, FileInfo file)
            {
                logger.LogInformation("Inserting Moving Model LOD {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoMovingModelLod(cdbName, movingModelLod, fileStream);
            }
            void tileAction(Tile tile, FileInfo file)
            {
                logger.LogInformation("Inserting Tile {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoTile(cdbName, tile, fileStream);

                if (CultureInfo.InvariantCulture.CompareInfo.Compare(tile.FileType, "zip", CompareOptions.IgnoreCase) == 0)
                {
                    using ZipArchive zipArchive = ZipFile.OpenRead(file.FullName);
                    foreach (var entry in zipArchive.Entries)
                    {
                        /*
                         * Unfortunately, file names that match the "feature code" pattern
                         * can also match the "texture name" pattern, because it just groups
                         * everything after the known stuff as the name of a texture.
                         * Therefore, order is crucial here.
                         */
                        Match featureMatch = TileArchivedFeature.ArchivedFilenamePattern.Match(entry.Name);
                        if (featureMatch.Success)
                        {
                            TileArchivedFeature tileArchivedFeature = TileArchivedFeature.FromArchivedFilenameMatch(featureMatch);

                            using Stream content = entry.Open();
                            sqlDataStore.InsertIntoTileArchivedFeature(cdbName, tileArchivedFeature, content);
                        }
                        else
                        {
                            Match textureMatch = TileArchivedTexture.ArchivedFilenamePattern.Match(entry.Name);
                            if (textureMatch.Success)
                            {
                                TileArchivedTexture tileArchivedTexture = TileArchivedTexture.FromArchivedFilenameMatch(textureMatch);

                                using Stream content = entry.Open();
                                sqlDataStore.InsertIntoTileArchivedTexture(cdbName, tileArchivedTexture, content);
                            }
                            else
                            {
                                // Unrecognized file, ignore it.
                            }
                        }
                    }
                }
            }
            void navigationAction(Navigation navigation, FileInfo file)
            {
                logger.LogInformation("Inserting Navigation {File}", file);
                using FileStream fileStream = new(file.FullName, fileStreamOptions);
                int rowsAffected = sqlDataStore.InsertIntoNavigation(cdbName, navigation, fileStream);
            }

            sqlDataStore.InsertIntoCDB(cdbName);

            fileSystemCDB.WalkAllFiles(
                metadataAction,
                textureAction,
                textureLodAction,
                geotypicalModelAction,
                geotypicalModelLodAction,
                movingModelAction,
                movingModelLodAction,
                tileAction,
                null,
                null,
                navigationAction);
        }

        DateTimeOffset end = DateTimeOffset.UtcNow;

        Console.WriteLine("Import time: {0}", end - start);

        CDBInformation sqlCDBInformation = new();
        sqlCDBInformation.Initialize(sqlCDB);

        foreach ((int code, string name) in sqlCDBInformation.DatasetNames)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:D3}_{1}", code, name));
        }
        foreach (FeatureCode featureCode in sqlCDBInformation.ValidFeatureSubcodes.Keys)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} = {1}, {2}, {3}",
                featureCode.Code,
                sqlCDBInformation.FeatureCategoryNames[featureCode.Category],
                sqlCDBInformation.FeatureSubcategoryNames[featureCode.Category + featureCode.Subcategory],
                sqlCDBInformation.FeatureTypeNames[featureCode]));
        }
    }
}
