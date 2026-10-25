using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;

namespace SistemaTarefas.Application.Mapeamentos;

public static class TarefaMappings
{
    public static TarefaResponse ParaResponse(this Tarefa tarefa, TimeProvider relogio)
        => new()
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo.Valor,
            Descricao = tarefa.Descricao.Valor,
            Status = tarefa.Status.ToString(),
            DataVencimento = tarefa.DataVencimento,
            DataCriacao = tarefa.DataCriacao,
            DataAtualizacao = tarefa.DataAtualizacao,
            DataConclusao = tarefa.DataConclusao,
            EstaAtrasada = tarefa.EstaAtrasada(relogio),
            Versao = tarefa.Versao
        };
}
