using FluentValidation;
using SistemaTarefas.Communication.Requests;

namespace SistemaTarefas.Application.Validadores;

public sealed class RequestRegistrarValidator : AbstractValidator<RequestRegistrar>
{
    public RequestRegistrarValidator()
    {
        RuleFor(x => x.NomeUsuario).NotEmpty().MinimumLength(3).MaximumLength(80);
        RuleFor(x => x.Senha)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Za-z]").WithMessage("A senha deve conter ao menos uma letra.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}
