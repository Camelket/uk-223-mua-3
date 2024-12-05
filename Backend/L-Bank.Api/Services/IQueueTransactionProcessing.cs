using System;

namespace L_Bank.Api.Services;

public interface IQueueTransactionProcessing
{
    Task<T> RegisterTransaction<T>(Func<Task<T>> transaction, IEnumerable<int> affectedIds);
}
