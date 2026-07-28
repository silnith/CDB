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

        CDBInformation sqlCDBInformation = new();
        sqlCDBInformation.Initialize(sqlCDB);

        CDBInformation fileSystemCDBInformation = new();
        fileSystemCDBInformation.Initialize(fileSystemCDB);

        DateTimeOffset start = DateTimeOffset.UtcNow;

        sqlDataStore.InsertIntoCDB(cdbName);
        foreach ((ICDBIdentifier id, Stream stream) in fileSystemCDB.EnumerateFiles())
        {
            id.WriteToCDB(sqlCDB, stream);
        }

        DateTimeOffset end = DateTimeOffset.UtcNow;

        Console.WriteLine("Import time: {0}", end - start);
    }
}
