using System.Runtime.CompilerServices;
using Contratos.Abstra��es.Mensagens;
using Domain.Abstractions.EventStore;

namespace Infraestrutura.ArmazenamentoDeEventos.Reposit�rios;

public interface IEventStoreRepository
{
    Task AcrescentarEventoAsync(StoreEvent armazenamentoDoEvento, CancellationToken tokenDeCancelamento);
    Task<List<IEvent>> ObterFluxoDeEventosAsync(Guid idDoAgregado, long? vers�o, CancellationToken tokenDeCancelamento);
    Task ExecutarTransa��oAsync(Func<CancellationToken, Task> opera��oAss�ncrona, CancellationToken tokenDeCancelamento);
}