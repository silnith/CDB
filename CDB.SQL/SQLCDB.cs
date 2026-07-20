using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

/// <summary>
/// A CDB data store that uses SQL as the backing store.
/// </summary>
public class SQLCDB : ICDB
{
    private readonly SQLDataStore sqlDataStore;

    /// <summary>
    /// Creates a new CDB data store that reads from the specified SQL database.
    /// </summary>
    /// <param name="sqlDataStore">An SQL data store implementation for a specific database.</param>
    /// <param name="options">Configurable settings.</param>
    public SQLCDB(SQLDataStore sqlDataStore, IOptions<SQLCDBSettings> options)
    {
        this.sqlDataStore = sqlDataStore;
        Name = options.Value.Name;
    }

    /// <summary>
    /// A simple identifier for the CDB data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This must match one of the values stored in the <c>CDB</c> table.
    /// </para>
    /// </remarks>
    public string Name
    {
        get;
    }

    private static readonly Regex PathPrefixPattern = new(
        @"^[\\/]?(?<directory>Metadata|GTModel|MModel|Tiles|Navigation)[\\/]",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <inheritdoc/>
    public bool TryReadFile(string filePathAndName, Action<Stream> fileFoundAction)
    {
        Match pathPrefixMatch = PathPrefixPattern.Match(filePathAndName);
        if (pathPrefixMatch.Success)
        {
            string directory = pathPrefixMatch.Groups["directory"].Value;
            return directory.ToLowerInvariant() switch
            {
                "metadata" => TryReadMetadata(filePathAndName, fileFoundAction),
                "gtmodel" => TryReadGeotypicalModel(filePathAndName, fileFoundAction),
                "mmodel" => TryReadMovingModel(filePathAndName, fileFoundAction),
                "tiles" => TryReadTile(filePathAndName, fileFoundAction),
                "navigation" => TryReadNavigation(filePathAndName, fileFoundAction),
                _ => false,
            };
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<bool> TryReadFileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        Match pathPrefixMatch = PathPrefixPattern.Match(filePathAndName);
        if (pathPrefixMatch.Success)
        {
            string directory = pathPrefixMatch.Groups["directory"].Value;
            return directory.ToLowerInvariant() switch
            {
                "metadata" => TryReadMetadataAsync(filePathAndName, fileFoundAsyncAction, cancellationToken),
                "gtmodel" => TryReadGeotypicalModelAsync(filePathAndName, fileFoundAsyncAction, cancellationToken),
                "mmodel" => TryReadMovingModelAsync(filePathAndName, fileFoundAsyncAction, cancellationToken),
                "tiles" => TryReadTileAsync(filePathAndName, fileFoundAsyncAction, cancellationToken),
                "navigation" => TryReadNavigationAsync(filePathAndName, fileFoundAsyncAction, cancellationToken),
                _ => Task.FromResult(false),
            };
        }
        else
        {
            return Task.FromResult(false);
        }
    }

    private bool TryReadMetadata(string filePathAndName, Action<Stream> fileFoundAction)
    {
        Metadata metadata = new(
            Path.GetFileNameWithoutExtension(filePathAndName),
            Path.GetExtension(filePathAndName).Substring(1));
        return sqlDataStore.TrySelectFromMetadata(Name, metadata, fileFoundAction);
    }

    private Task<bool> TryReadMetadataAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        Metadata metadata = new(
            Path.GetFileNameWithoutExtension(filePathAndName),
            Path.GetExtension(filePathAndName).Substring(1));
        return sqlDataStore.TrySelectFromMetadataAsync(Name, metadata, fileFoundAsyncAction, cancellationToken);
    }

    private bool TryReadGeotypicalModel(string filePathAndName, Action<Stream> fileFoundAction)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match geotypicalModelLodMatch = GeotypicalModelLod.FilenamePattern.Match(filename);
        if (geotypicalModelLodMatch.Success)
        {
            GeotypicalModelLod geotypicalModelLod = GeotypicalModelLod.FromFilenameMatch(geotypicalModelLodMatch);
            return sqlDataStore.TrySelectFromGeotypicalModelLod(Name, geotypicalModelLod, fileFoundAction);
        }
        Match geotypicalModelMatch = GeotypicalModel.FilenamePattern.Match(filename);
        if (geotypicalModelMatch.Success)
        {
            GeotypicalModel geotypicalModel = GeotypicalModel.FromFilenameMatch(geotypicalModelMatch);
            return sqlDataStore.TrySelectFromGeotypicalModel(Name, geotypicalModel, fileFoundAction);
        }
        Match textureLodMatch = TextureLod.FilenamePattern.Match(filename);
        if (textureLodMatch.Success)
        {
            TextureLod textureLod = TextureLod.FromFilenameMatch(textureLodMatch);
            return sqlDataStore.TrySelectFromTextureLod(Name, textureLod, fileFoundAction);
        }
        Match textureMatch = Texture.FilenamePattern.Match(filename);
        if (textureMatch.Success)
        {
            Texture texture = Texture.FromFilenameMatch(textureMatch);
            return sqlDataStore.TrySelectFromTexture(Name, texture, fileFoundAction);
        }
        return false;
    }

    private Task<bool> TryReadGeotypicalModelAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match geotypicalModelLodMatch = GeotypicalModelLod.FilenamePattern.Match(filename);
        if (geotypicalModelLodMatch.Success)
        {
            GeotypicalModelLod geotypicalModelLod = GeotypicalModelLod.FromFilenameMatch(geotypicalModelLodMatch);
            return sqlDataStore.TrySelectFromGeotypicalModelLodAsync(Name, geotypicalModelLod, fileFoundAsyncAction, cancellationToken);
        }
        Match geotypicalModelMatch = GeotypicalModel.FilenamePattern.Match(filename);
        if (geotypicalModelMatch.Success)
        {
            GeotypicalModel geotypicalModel = GeotypicalModel.FromFilenameMatch(geotypicalModelMatch);
            return sqlDataStore.TrySelectFromGeotypicalModelAsync(Name, geotypicalModel, fileFoundAsyncAction, cancellationToken);
        }
        Match textureLodMatch = TextureLod.FilenamePattern.Match(filename);
        if (textureLodMatch.Success)
        {
            TextureLod textureLod = TextureLod.FromFilenameMatch(textureLodMatch);
            return sqlDataStore.TrySelectFromTextureLodAsync(Name, textureLod, fileFoundAsyncAction, cancellationToken);
        }
        Match textureMatch = Texture.FilenamePattern.Match(filename);
        if (textureMatch.Success)
        {
            Texture texture = Texture.FromFilenameMatch(textureMatch);
            return sqlDataStore.TrySelectFromTextureAsync(Name, texture, fileFoundAsyncAction, cancellationToken);
        }
        return Task.FromResult(false);
    }

    private bool TryReadMovingModel(string filePathAndName, Action<Stream> fileFoundAction)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match movingModelLodMatch = MovingModelLod.FilenamePattern.Match(filename);
        if (movingModelLodMatch.Success)
        {
            MovingModelLod movingModelLod = MovingModelLod.FromFilenameMatch(movingModelLodMatch);
            return sqlDataStore.TrySelectFromMovingModelLod(Name, movingModelLod, fileFoundAction);
        }
        Match movingModelMatch = MovingModel.FilenamePattern.Match(filename);
        if (movingModelMatch.Success)
        {
            MovingModel movingModel = MovingModel.FromFilenameMatch(movingModelMatch);
            return sqlDataStore.TrySelectFromMovingModel(Name, movingModel, fileFoundAction);
        }
        Match textureLodMatch = TextureLod.FilenamePattern.Match(filename);
        if (textureLodMatch.Success)
        {
            TextureLod textureLod = TextureLod.FromFilenameMatch(textureLodMatch);
            return sqlDataStore.TrySelectFromTextureLod(Name, textureLod, fileFoundAction);
        }
        Match textureMatch = Texture.FilenamePattern.Match(filename);
        if (textureMatch.Success)
        {
            Texture texture = Texture.FromFilenameMatch(textureMatch);
            return sqlDataStore.TrySelectFromTexture(Name, texture, fileFoundAction);
        }
        return false;
    }

    private Task<bool> TryReadMovingModelAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match movingModelLodMatch = MovingModelLod.FilenamePattern.Match(filename);
        if (movingModelLodMatch.Success)
        {
            MovingModelLod movingModelLod = MovingModelLod.FromFilenameMatch(movingModelLodMatch);
            return sqlDataStore.TrySelectFromMovingModelLodAsync(Name, movingModelLod, fileFoundAsyncAction, cancellationToken);
        }
        Match movingModelMatch = MovingModel.FilenamePattern.Match(filename);
        if (movingModelMatch.Success)
        {
            MovingModel movingModel = MovingModel.FromFilenameMatch(movingModelMatch);
            return sqlDataStore.TrySelectFromMovingModelAsync(Name, movingModel, fileFoundAsyncAction, cancellationToken);
        }
        Match textureLodMatch = TextureLod.FilenamePattern.Match(filename);
        if (textureLodMatch.Success)
        {
            TextureLod textureLod = TextureLod.FromFilenameMatch(textureLodMatch);
            return sqlDataStore.TrySelectFromTextureLodAsync(Name, textureLod, fileFoundAsyncAction, cancellationToken);
        }
        Match textureMatch = Texture.FilenamePattern.Match(filename);
        if (textureMatch.Success)
        {
            Texture texture = Texture.FromFilenameMatch(textureMatch);
            return sqlDataStore.TrySelectFromTextureAsync(Name, texture, fileFoundAsyncAction, cancellationToken);
        }
        return Task.FromResult(false);
    }

    private bool TryReadTile(string filePathAndName, Action<Stream> fileFoundAction)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match tileArchivedFeatureMatch = TileArchivedFeature.ArchivedFilenamePattern.Match(filename);
        if (tileArchivedFeatureMatch.Success)
        {
            TileArchivedFeature tileArchivedFeature = TileArchivedFeature.FromArchivedFilenameMatch(tileArchivedFeatureMatch);
            return sqlDataStore.TrySelectFromTileArchivedFeature(Name, tileArchivedFeature, fileFoundAction);
        }
        Match tileArchivedTextureMatch = TileArchivedTexture.ArchivedFilenamePattern.Match(filename);
        if (tileArchivedTextureMatch.Success)
        {
            TileArchivedTexture tileArchivedTexture = TileArchivedTexture.FromArchivedFilenameMatch(tileArchivedTextureMatch);
            return sqlDataStore.TrySelectFromTileArchivedTexture(Name, tileArchivedTexture, fileFoundAction);
        }
        Match tileMatch = Tile.TiledDatasetFilenamePattern.Match(filename);
        if (tileMatch.Success)
        {
            Tile tile = Tile.FromTiledDatasetFilenameMatch(tileMatch);
            return sqlDataStore.TrySelectFromTile(Name, tile, fileFoundAction);
        }
        return false;
    }

    private Task<bool> TryReadTileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match tileArchivedFeatureMatch = TileArchivedFeature.ArchivedFilenamePattern.Match(filename);
        if (tileArchivedFeatureMatch.Success)
        {
            TileArchivedFeature tileArchivedFeature = TileArchivedFeature.FromArchivedFilenameMatch(tileArchivedFeatureMatch);
            return sqlDataStore.TrySelectFromTileArchivedFeatureAsync(Name, tileArchivedFeature, fileFoundAsyncAction, cancellationToken);
        }
        Match tileArchivedTextureMatch = TileArchivedTexture.ArchivedFilenamePattern.Match(filename);
        if (tileArchivedTextureMatch.Success)
        {
            TileArchivedTexture tileArchivedTexture = TileArchivedTexture.FromArchivedFilenameMatch(tileArchivedTextureMatch);
            return sqlDataStore.TrySelectFromTileArchivedTextureAsync(Name, tileArchivedTexture, fileFoundAsyncAction, cancellationToken);
        }
        Match tileMatch = Tile.TiledDatasetFilenamePattern.Match(filename);
        if (tileMatch.Success)
        {
            Tile tile = Tile.FromTiledDatasetFilenameMatch(tileMatch);
            return sqlDataStore.TrySelectFromTileAsync(Name, tile, fileFoundAsyncAction, cancellationToken);
        }
        return Task.FromResult(false);
    }

    private bool TryReadNavigation(string filePathAndName, Action<Stream> fileFoundAction)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match navigationMatch = Navigation.FilenamePattern.Match(filename);
        if (navigationMatch.Success)
        {
            Navigation navigation = Navigation.FromFilenameMatch(navigationMatch);
            return sqlDataStore.TrySelectFromNavigation(Name, navigation, fileFoundAction);
        }
        return false;
    }

    private Task<bool> TryReadNavigationAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        string filename = Path.GetFileName(filePathAndName);
        Match navigationMatch = Navigation.FilenamePattern.Match(filename);
        if (navigationMatch.Success)
        {
            Navigation navigation = Navigation.FromFilenameMatch(navigationMatch);
            return sqlDataStore.TrySelectFromNavigationAsync(Name, navigation, fileFoundAsyncAction, cancellationToken);
        }
        return Task.FromResult(false);
    }

    #region Dispose Pattern

    private bool disposedValue;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> if the call came from a
    /// <see cref="IDisposable.Dispose"/> or <see cref="IAsyncDisposable.DisposeAsync"/> method,
    /// <see langword="false"/> if it came from a finalizer.</param>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                sqlDataStore.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

}
