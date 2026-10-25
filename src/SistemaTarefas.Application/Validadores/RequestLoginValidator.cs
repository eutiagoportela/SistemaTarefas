using FluentValidation;
using SistemaTarefas.Communication.Requests;

namespace SistemaTarefas.Application.Validadores;

public sealed class RequestLoginValidator : AbstractValidator<RequestLogin>
{
    public RequestLoginValidator()
    {
        RuleFor(x => x.NomeUsuario).NotEmpty();
        RuleFor(x => x.Senha).NotEmpty();
    }
}
