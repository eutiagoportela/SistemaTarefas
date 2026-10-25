namespace SistemaTarefas.Communication.Requests;

public class RequestLogin
{
    public string NomeUsuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
