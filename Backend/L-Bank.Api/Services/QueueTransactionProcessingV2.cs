using System.Collections.Concurrent;

namespace L_Bank.Api.Services;

internal class OutstandingTransaction
{
    internal required Func<Task> transaction;
    internal IEnumerable<int> affectedIds = [];
}

public class QueueTransactionProcessingV2 : IQueueTransactionProcessing
{
    private static int atWork;
    private static int readyForNextBatch;

    private const int MAX_BATCH_SIZE = 10;
    private const int MAX_NO_WORK_COUNT = 10;

    private readonly ConcurrentQueue<List<Func<Task>>> _batchQueue = new();
    private readonly ConcurrentQueue<OutstandingTransaction> _transactionQueue = new();

    private readonly ILogger<QueueTransactionProcessingV2> logger;

    public QueueTransactionProcessingV2(ILogger<QueueTransactionProcessingV2> logger)
    {
        this.logger = logger;
    }

    public Task<T> RegisterTransaction<T>(Func<Task<T>> transaction, IEnumerable<int> affectedIds)
    {
        var (item, task) = SetupOutstandingTransactionItem(transaction, affectedIds);

        StartWorker();

        _transactionQueue.Enqueue(item);
        logger.LogInformation("Registered Transaction");

        return task;
    }

    private void StartWorker()
    {
        bool won = Interlocked.CompareExchange(ref atWork, 1, 0) == 0;
        if (!won)
            return;

        Thread thread = new(DoWork);
        thread.Priority = ThreadPriority.Highest;
        logger.LogInformation("Started Thread");
        thread.Start(OnWorkerEnd);
    }

    private void OnWorkerEnd()
    {
        Interlocked.Exchange(ref atWork, 0);
    }

    private async void DoWork(object state)
    {
        logger.LogInformation("Starting Work");
        int noWorkCount = 0;
        while (noWorkCount < MAX_NO_WORK_COUNT)
        {
            await Task.Delay(2);
            logger.LogInformation("At Work");
            if (!_batchQueue.IsEmpty)
            {
                noWorkCount = 0;

                var _ = ProcessNextBatch();
            }

            if (!_transactionQueue.IsEmpty)
            {
                noWorkCount = 0;

                var _ = CreateNextBatch();
                continue;
            }

            noWorkCount++;
        }

        Action completeAction = (Action)state;
        completeAction.Invoke();

        logger.LogInformation("Finished Work");
    }

    private async Task ProcessNextBatch()
    {
        bool won = false;
        try
        {
            won = Interlocked.CompareExchange(ref readyForNextBatch, 1, 0) == 0;
            if (won)
            {
                if (_batchQueue.TryDequeue(out var batch))
                {
                    List<Task> tasks = [];
                    foreach (var transaction in batch)
                    {
                        tasks.Add(transaction());
                    }

                    await Task.WhenAll(tasks);
                    logger.LogInformation("Processed Batch");
                }
            }
        }
        finally
        {
            if (won)
                Interlocked.Exchange(ref readyForNextBatch, 0);
        }
    }

    private async Task CreateNextBatch()
    {
        await Task.Run(() =>
        {
            // await Task.Delay(5);
            HashSet<int> lockedIds = [];
            List<Func<Task>> batch = [];
            int unbatchablesInARow = 0;

            while (
                !_transactionQueue.IsEmpty
                && batch.Count < MAX_BATCH_SIZE
                && unbatchablesInARow < MAX_BATCH_SIZE
            )
            {
                if (_transactionQueue.TryDequeue(out var item))
                {
                    if (item.affectedIds.Any(lockedIds.Contains))
                    {
                        unbatchablesInARow++;
                        _transactionQueue.Enqueue(item);
                        continue;
                    }

                    unbatchablesInARow = 0;
                    batch.Add(item.transaction);
                    foreach (var ai in item.affectedIds)
                    {
                        lockedIds.Add(ai);
                    }
                }
            }

            if (batch.Count > 0)
            {
                _batchQueue.Enqueue(batch);
                logger.LogInformation($"Created Batch of Size: {batch.Count}");
            }
        });
    }

    private static (OutstandingTransaction item, Task<T> task) SetupOutstandingTransactionItem<T>(
        Func<Task<T>> transaction,
        IEnumerable<int> affectedIds
    )
    {
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

        return (
            new OutstandingTransaction() { transaction = callback, affectedIds = affectedIds },
            tcs.Task
        );
    }
}
