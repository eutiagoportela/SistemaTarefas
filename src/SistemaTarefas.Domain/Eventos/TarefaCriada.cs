namespace SistemaTarefas.Domain.Eventos;

public sealed record TarefaCriada(Guid TarefaId, Guid UsuarioId, DateTime OcorridoEm) : IDomainEvent;
