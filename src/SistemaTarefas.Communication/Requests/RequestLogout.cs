namespace SistemaTarefas.Communication.Requests;

public sealed class RequestLogout
{
    public string RefreshToken { get; set; } = string.Empty;
}
