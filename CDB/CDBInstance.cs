using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB;

/// <summary>
/// A CDB Configuration is an ordered list of CDB Versions.
/// </summary>
public class CDBInstance : ICDB
{
    private readonly List<ICDB> cdbs;

    /// <summary>
    /// Creates a CDB Configuration using the list of CDB Versions.
    /// </summary>
    /// <param name="cdbs">The CDB Versions, in order.</param>
    public CDBInstance(IEnumerable<ICDB> cdbs)
    {
        this.cdbs = cdbs.ToList();
    }

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadMetadata(metadata);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadMetadataAsync(metadata, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadTexture(texture);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadTextureAsync(texture, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadTextureLevelOfDetail(textureLod);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadTextureLevelOfDetailAsync(textureLod, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadGeotypicalModel(geotypicalModel);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadGeotypicalModelAsync(geotypicalModel, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadGeotypicalModelLevelOfDetail(geotypicalModelLod);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadGeotypicalModelLevelOfDetailAsync(geotypicalModelLod, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadMovingModel(movingModel);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadMovingModelAsync(movingModel, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadMovingModelLevelOfDetail(movingModelLod);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadMovingModelLevelOfDetailAsync(movingModelLod, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadTile(tile);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadTileAsync(tile, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileFeature)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadTileFeature(tileFeature);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileFeature, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadTileFeatureAsync(tileFeature, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileTexture)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadTileTexture(tileTexture);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileTexture, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadTileTextureAsync(tileTexture, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = cdb.ReadNavigation(navigation);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        foreach (ICDB cdb in cdbs)
        {
            Stream? stream = await cdb.ReadNavigationAsync(navigation, cancellationToken);
            if (stream is not null)
            {
                return stream;
            }
        }
        return null;
    }

    public Task<Stream?> ReadNavigation2Async(Navigation navigation, CancellationToken cancellationToken)
    {
        Task<Stream?> function(ICDB cdb, CancellationToken token)
        {
            return cdb.ReadNavigationAsync(navigation, cancellationToken);
        }

        return RunParallelFunctionAsync(function, cancellationToken);
    }

    /// <summary>
    /// Runs an asynchronous function in parallel on all CDBs, and returns the
    /// first non-null result, discarding all the rest.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Stream?> RunParallelFunctionAsync(Func<ICDB, CancellationToken, Task<Stream?>> function, CancellationToken cancellationToken)
    {
        CancellationTokenSource leftoverTasks = new();
        CancellationToken leftoverTasksCancellationToken = leftoverTasks.Token;
        Queue<Task<Stream?>> tasks = new();
        foreach (ICDB cdb in cdbs)
        {
            Task<Stream?> boundParametersFunction()
            {
                return function(cdb, leftoverTasksCancellationToken);
            }

            Task<Stream?> task = Task.Run(boundParametersFunction, leftoverTasksCancellationToken);
            tasks.Enqueue(task);
        }

        UnusedStreamResultDisposer MapToDisposer(Task<Stream?> task)
        {
            return new(task);
        }

        while (!cancellationToken.IsCancellationRequested
            && tasks.TryDequeue(out Task<Stream?>? task))
        {
            try
            {
                Stream? stream = await task;
                if (stream is not null)
                {
                    leftoverTasks.Cancel(false);

                    return new WrappedStream(stream, tasks.Select(MapToDisposer).ToArray());
                }
            }
            catch (Exception)
            {
            }
        }
        leftoverTasks.Cancel(false);
        ValueTask DisposeUnused(UnusedStreamResultDisposer task, CancellationToken _)
        {
            return task.DisposeAsync();
        }

        await Parallel.ForEachAsync(tasks.Select(MapToDisposer), DisposeUnused);
        return null;
    }

    private class UnusedStreamResultDisposer : IDisposable, IAsyncDisposable
    {
        private readonly Task<Stream?>? task;

        public UnusedStreamResultDisposer(Task<Stream?>? task)
        {
            this.task = task;
        }

        public void Dispose()
        {
            if (task is not null)
            {
                try
                {
                    Stream? stream = task.GetAwaiter().GetResult();
                    stream?.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (task is not null)
            {
                try
                {
                    Stream? stream = await task;
                    if (stream is not null)
                    {
                        await stream.DisposeAsync();
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }

    /// <inheritdoc/>
    public bool TryReadFile(string filePathAndName, Action<Stream> fileFoundAction)
    {
        foreach (ICDB cdb in cdbs)
        {
            if (cdb.TryReadFile(filePathAndName, fileFoundAction))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Walks the list of CDB versions and finds the first containing the
    /// specified file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This initiates file open operations in all CDB versions simultaneously
    /// using the default thread pool.  However, it inserts a barrier after the
    /// file is opened but before it is passed to the <paramref name="fileFoundAsyncAction"/>,
    /// so it can order the found files and pass the correct one to the action.
    /// </para>
    /// <para>
    /// If no file is found, the action is not called.
    /// </para>
    /// </remarks>
    /// <param name="filePathAndName">The relative path and filename of the file to read.
    /// The path should be relative to the CDB root.</param>
    /// <param name="fileFoundAsyncAction">The action to run if the file is found.
    /// The stream will be automatically closed after the action returns or
    /// throws an exception.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true"/> if the file was found.</returns>
    public async Task<bool> TryReadFileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        /*
         * This cancellation token source is purely to cancel any pending or
         * partially-completed read requests that are no longer necessary once
         * a higher priority read has completed.
         */
        CancellationTokenSource leftoverTasks = new();
        CancellationToken leftoverTasksCancellationToken = leftoverTasks.Token;
        /*
         * First invoke a parallel read request for each CDB in the list of CDB
         * versions.
         */
        Queue<(Task<bool>, TaskCompletionSource)> queue = new();
        foreach (ICDB cdb in cdbs)
        {
            TaskCompletionSource barrierSource = new();
            Task barrierTask = barrierSource.Task;
            /*
             * The purpose of this local function is to insert a barrier before
             * the execution of the client-supplied action.
             * 
             * We need the client-supplied action to execute only once.  But we
             * are passing the action to many downstream asynchronous function
             * calls.  The barriers prevent any of them from executing the
             * function until later when we walk the list of downstream
             * functions and allow one to proceed at a time.
             * 
             * The point of all this is so that the setup work for reading each
             * potential file can happen in parallel, but we block the actual
             * processing of the opened files until we can walk them in the
             * proper sequence.
             */
            async Task wrappedAsyncAction(Stream stream, CancellationToken leftoverTaskToken)
            {
                await barrierTask;
                if (leftoverTaskToken.IsCancellationRequested)
                {
                    return;
                }
                /*
                 * The user-supplied action will only ever see the user-supplied cancellation token.
                 */
                await fileFoundAsyncAction(stream, cancellationToken);
            }

            /*
             * The sole purpose of this local function is to bind the parameters to the TryReadFileAsync call.
             */
            Task<bool> callTryReadFileAsync()
            {
                /*
                 * The wrappedAsyncAction receives the token from the cancellationTokenSource
                 * instead of the cancellation token passed in by the caller.
                 */
                return cdb.TryReadFileAsync(filePathAndName, wrappedAsyncAction, leftoverTasksCancellationToken);
            }

            Task<bool> task = Task.Run(callTryReadFileAsync, leftoverTasksCancellationToken);
            (Task<bool>, TaskCompletionSource) tuple = (task, barrierSource);
            queue.Enqueue(tuple);
        }
        /*
         * Next, walk the file read operations in order and allow them access to
         * the processing action provided by the caller, one at a time.
         * If any one of them succeeds, cease calling any others.
         */
        bool success = false;
        while (queue.TryDequeue(out (Task<bool>, TaskCompletionSource) tuple))
        {
            (Task<bool> readTask, TaskCompletionSource barrierSource) = tuple;
            /*
             * This allows the wrappedAsyncAction to proceed, if it was ever called.
             * If not, the task will return false anyway.
             */
            barrierSource.SetResult();
            if (await readTask)
            {
                success = true;
                /*
                 * One of the files was found and processed.
                 * Cancel all the other pending operations.
                 */
                leftoverTasks.Cancel();
                break;
            }
        }
        /*
         * Finally, walk the file read operations that were never allowed to
         * proceed, and cancel all the barrier tasks.  This allows them to exit.
         */
        while (queue.TryDequeue(out (Task<bool>, TaskCompletionSource) tuple))
        {
            (Task<bool> _, TaskCompletionSource barrierSource) = tuple;
            /*
             * We could either cancel the barrier task, or set it as completed.
             * If completed, the wrappedAsyncAction will proceed and find that
             * the leftover task token is cancelled, so it will exit.
             * If cancelled, the wrappedAsyncAction will throw a task cancelled
             * exception, and exit.
             * Either way, the delegate will not be called.
             */
            _ = barrierSource.TrySetCanceled(leftoverTasksCancellationToken);
            //_ = barrierSource.TrySetResult();
        }
        return success;
    }

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        cdbs.First().WriteMetadata(metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteMetadataAsync(metadata, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        cdbs.First().WriteTexture(texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteTextureAsync(texture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        cdbs.First().WriteTextureLevelOfDetail(textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteTextureLevelOfDetailAsync(textureLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        cdbs.First().WriteGeotypicalModel(geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteGeotypicalModelAsync(geotypicalModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        cdbs.First().WriteGeotypicalModelLevelOfDetail(geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteGeotypicalModelLevelOfDetailAsync(geotypicalModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        cdbs.First().WriteMovingModel(movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteMovingModelAsync(movingModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        cdbs.First().WriteMovingModelLevelOfDetail(movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteMovingModelLevelOfDetailAsync(movingModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        cdbs.First().WriteTile(tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteTileAsync(tile, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileFeature, Stream content)
    {
        cdbs.First().WriteTileFeature(tileFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileFeature, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteTileFeatureAsync(tileFeature, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileTexture, Stream content)
    {
        cdbs.First().WriteTileTexture(tileTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileTexture, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteTileTextureAsync(tileTexture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        cdbs.First().WriteNavigation(navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return cdbs.First().WriteNavigationAsync(navigation, content, cancellationToken);
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
                foreach (ICDB cdb in cdbs)
                {
                    cdb.Dispose();
                }
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }

    #endregion

    #region Async Dispose Pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync"/>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await Task.WhenAll(cdbs.Select(cdb => cdb.DisposeAsync().AsTask()));
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Perform async cleanup.
        await DisposeAsyncCore();

        // Dispose of unmanaged resources.
        Dispose(false);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    #endregion

}
