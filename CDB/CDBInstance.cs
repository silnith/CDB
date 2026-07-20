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

    public CDBInstance(IEnumerable<ICDB> cdbs)
    {
        this.cdbs = cdbs.ToList();
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
