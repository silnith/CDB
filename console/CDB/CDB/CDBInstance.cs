using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB;

public class CDBInstance : ICDB
{
    private readonly List<ICDB> cdbs;

    public CDBInstance(string name, DirectoryInfo root, IEnumerable<ICDB> cdbs)
    {
        Name = name;
        CdbRoot = root;
        this.cdbs = cdbs.ToList();
    }

    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <inheritdoc/>
    public DirectoryInfo CdbRoot
    {
        get;
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

    /// <inheritdoc/>
    public async Task<bool> TryReadFileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        /*
         * This cancellation token source is purely to cancel any pending or
         * partially-completed read requests that are no longer necessary once
         * a higher priority read has completed.
         */
        CancellationTokenSource cancellationTokenSource = new();
        /*
         * First invoke a parallel read request for each CDB in the list of CDB
         * versions.
         */
        Queue<(Task<bool>, TaskCompletionSource)> queue = new();
        foreach (ICDB cdb in cdbs)
        {
            TaskCompletionSource taskCompletionSource = new();
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
            async Task wrappedAsyncAction(Stream stream, CancellationToken token)
            {
                await taskCompletionSource.Task;
                if (!token.IsCancellationRequested)
                {
                    await fileFoundAsyncAction(stream, cancellationToken);
                }
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
                return cdb.TryReadFileAsync(filePathAndName, wrappedAsyncAction, cancellationTokenSource.Token);
            }

            Task<bool> task = Task.Run(callTryReadFileAsync, cancellationTokenSource.Token);
            (Task<bool>, TaskCompletionSource) item = (task, taskCompletionSource);
            queue.Enqueue(item);
        }
        /*
         * Next, walk the file read operations in order and all them access to
         * the processing action provided by the caller, one at a time.
         * If any one of them succeeds, cease calling any others.
         */
        bool success = false;
        while (queue.TryDequeue(out (Task<bool>, TaskCompletionSource) tuple))
        {
            (Task<bool> task, TaskCompletionSource taskCompletionSource) = tuple;
            /*
             * This allows the wrappedAsyncAction to proceed, if it was ever called.
             */
            taskCompletionSource.SetResult();
            if (await task)
            {
                success = true;
                /*
                 * One of the files was found and processed.
                 * Cancel all the other pending operations.
                 */
                cancellationTokenSource.Cancel();
                break;
            }
        }
        /*
         * Finally, walk the file read operations that were never allowed to
         * proceed, and cancel them all.
         */
        while (queue.TryDequeue(out (Task<bool>, TaskCompletionSource) tuple))
        {
            (Task<bool> _, TaskCompletionSource taskCompletionSource) = tuple;
            _ = taskCompletionSource.TrySetCanceled(cancellationTokenSource.Token);
        }
        return success;
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

}
