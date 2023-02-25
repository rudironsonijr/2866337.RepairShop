using System.Runtime.CompilerServices;
using Contracts.Abstractions.Messages;
using Domain.Abstractions.EventStore;
using Infrastructure.EventStore.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EventStore;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly EventStoreDbContext _dbContext;

    public EventStoreRepository(EventStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AppendEventAsync(StoreEvent storeEvent, CancellationToken cancellationToken)
    {
        await _dbContext.Set<StoreEvent>().AddAsync(storeEvent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }


    public Task<List<IDomainEvent>> GetStreamAsync(Guid aggregateId, long? version, CancellationToken cancellationToken)
        => _dbContext.Set<StoreEvent>()
            .AsNoTracking()
            .Where(@event => @event.AggregateId.Equals(aggregateId))
            .Where(@event => @event.Version > (version ?? 0))
            .Select(@event => @event.Event)
            .ToListAsync(cancellationToken);

    public ConfiguredCancelableAsyncEnumerable<Guid> StreamAggregatesId(CancellationToken cancellationToken)
        => _dbContext.Set<StoreEvent>()
            .AsNoTracking()
            .Select(@event => @event.AggregateId)
            .Distinct()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

    public Task ExecuteTransactionAsync(Func<CancellationToken, Task> operationAsync, CancellationToken cancellationToken)
        => _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(ct => OnExecuteTransactionAsync(operationAsync, ct), cancellationToken);

    private async Task OnExecuteTransactionAsync(Func<CancellationToken, Task> operationAsync, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operationAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}