namespace SistemaTarefas.Domain.Eventos;

public sealed record TarefaConcluida(Guid TarefaId, Guid UsuarioId, DateTime OcorridoEm) : IDomainEvent;
