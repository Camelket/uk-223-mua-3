using System.Collections.Concurrent;

namespace L_Bank.Api.Services;

internal class NextBatchWrapper
{
    internal required Func<Task> transaction;
    internal IEnumerable<int> affectedIds = [];
}

/// <summary>
/// This is an old Version. See new at: QueueTransactionProcessingV2
/// </summary>
public class QueueTransactionProcessing : IQueueTransactionProcessing
{
    private static QueueTransactionProcessing instance;
    private ConcurrentBag<int> idsInProcessing = [];
    private BlockingCollection<Func<Task>> currentBatch = new();
    private ConcurrentQueue<NextBatchWrapper> nextBatch = new();

    private bool clockIsRunning = false;
    private int unusedCycleCounter = 0;
    System.Timers.Timer aTimer;
    private bool inProcess = false;

    private ILogger<QueueTransactionProcessing> logger;

    public QueueTransactionProcessing(ILogger<QueueTransactionProcessing> logger)
    {
        this.logger = logger;
        instance = this;
    }

    private void ManageClock()
    {
        if (clockIsRunning)
        {
            return;
        }

        logger.LogDebug("New Clock");
        clockIsRunning = true;

        aTimer = new System.Timers.Timer();
        aTimer.Interval = 100;
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
    {
        instance.logger.LogDebug("Timer called");
        if (instance.inProcess)
        {
            return;
        }

        instance.logger.LogDebug("Starting Processing");
        instance.ProcessBatch();
    }

    public Task<T> RegisterTransaction<T>(Func<Task<T>> transaction, IEnumerable<int> affectedIds)
    {
        ManageClock();
        logger.LogDebug("New Registration");

        var tcs = new TaskCompletionSource<T>();
        var callback = async () =>
        {
            try
            {
                T res = await transaction();
                if (res != null)
                {
                    _ = tcs.TrySetResult(res);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (OperationCanceledException ex)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        };

        if (currentBatch.IsAddingCompleted || affectedIds.Any(ai => idsInProcessing.Contains(ai)))
        {
            logger.LogDebug("Adding to nextBatch");
            nextBatch.Enqueue(
                new NextBatchWrapper() { transaction = callback, affectedIds = affectedIds }
            );
            logger.LogDebug("Added to nextBatch");
        }
        else
        {
            logger.LogDebug("Adding affectedIds to idsInProcessing");
            foreach (var ai in affectedIds)
            {
                idsInProcessing.Add(ai);
            }
            logger.LogDebug("Added affectedIds to idsInProcessing");
            logger.LogDebug("Adding to currentBatch");
            currentBatch.Add(callback);
            logger.LogDebug("Added to currentBatch");
        }

        logger.LogDebug("Returning Task");
        return tcs.Task;
    }

    private async void ProcessBatch()
    {
        inProcess = true;
        currentBatch.CompleteAdding();

        logger.LogWarning($"{currentBatch.Count} B");
        logger.LogWarning($"{nextBatch.Count} Q");

        if (currentBatch.Count == 0 && nextBatch.IsEmpty)
        {
            Interlocked.Increment(ref unusedCycleCounter);
            if (unusedCycleCounter > 10)
            {
                aTimer.Stop();
                Interlocked.Exchange(ref unusedCycleCounter, 0);
                clockIsRunning = false;
                logger.LogDebug("Stopped Clock");
            }
            inProcess = false;
            idsInProcessing = [];
            currentBatch = new();
            return;
        }

        List<Task> tasks = [];
        foreach (var value in currentBatch.GetConsumingEnumerable())
        {
            tasks.Add(value());
        }

        await Task.WhenAll(tasks);
        logger.LogDebug("Processed");

        inProcess = false;
        idsInProcessing = [];
        currentBatch = new();

        WorkOnQueue();
    }

    private async void WorkOnQueue()
    {
        await Task.Run(() =>
        {
            do
            {
                if (nextBatch.TryDequeue(out var item))
                {
                    if (
                        currentBatch.IsAddingCompleted
                        || item.affectedIds.Any(ai => idsInProcessing.Contains(ai))
                    )
                    {
                        nextBatch.Enqueue(item);
                    }
                    else
                    {
                        foreach (var ai in item.affectedIds)
                        {
                            idsInProcessing.Add(ai);
                        }
                        currentBatch.Add(item.transaction);
                    }
                }
            } while (!nextBatch.IsEmpty && !currentBatch.IsAddingCompleted);
        });
    }
}
