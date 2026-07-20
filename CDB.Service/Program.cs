using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Silnith.CDB.FileSystem;
using Silnith.CDB.SQL;
using Silnith.CDB.SQL.SQLite;
using System.Data.Common;
using System.Threading.Tasks;

namespace Silnith.CDB.Service;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        //builder.Services.AddSingleton<ICDB, FileSystemCDB>();
        //builder.Services.AddOptions<FileSystemCDBSettings>()
        //    .Configure(settings =>
        //    {
        //        settings.Root = new("CDB");
        //    });
        builder.Services.AddSingleton(provider =>
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new()
            {
                Cache = SqliteCacheMode.Default,
                DataSource = "CDB.db",
                ForeignKeys = true,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = true,
                RecursiveTriggers = true
            };
            SqliteConnection sqliteConnection = new(sqliteConnectionStringBuilder.ConnectionString);
            sqliteConnection.Open();
            return sqliteConnection;
        });
        builder.Services.AddSingleton<DbDataSource, SQLiteDataSource>();
        builder.Services.AddSingleton<SQLDataStore, SQLiteDataStore>();
        builder.Services.AddOptions<SQLiteDataStoreSettings>()
            .Configure(settings =>
            {
                settings.CreateSchema = false;
            });
        builder.Services.AddSingleton<ICDB, SQLCDB>();
        builder.Services.AddOptions<SQLCDBSettings>()
            .Configure(settings =>
            {
                settings.Name = "CDB";
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        //app.UseHttpsRedirection();

        //app.UseAuthorization();


        app.MapControllers();

        await app.RunAsync();
    }
}
