namespace SistemaTarefas.Domain.Eventos;

public sealed record TarefaReaberta(Guid TarefaId, Guid UsuarioId, DateTime OcorridoEm) : IDomainEvent;
