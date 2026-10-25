using FluentValidation;
using SistemaTarefas.Communication.Requests;

namespace SistemaTarefas.Application.Validadores;

public sealed class RequestAtualizarTarefaValidator : AbstractValidator<RequestAtualizarTarefa>
{
    public RequestAtualizarTarefaValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().Length(3, 120);
        RuleFor(x => x.Descricao).MaximumLength(1000);
        RuleFor(x => x.DataVencimento).NotEmpty();
    }
}
