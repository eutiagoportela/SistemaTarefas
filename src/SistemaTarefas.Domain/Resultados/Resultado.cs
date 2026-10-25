namespace SistemaTarefas.Domain.Resultados;

public sealed class Resultado
{
    public bool Sucesso { get; }
    public ErroAplicacao? Erro { get; }

    private Resultado(bool sucesso, ErroAplicacao? erro)
    {
        Sucesso = sucesso;
        Erro = erro;
    }

    public static Resultado Ok() => new(true, null);

    public static Resultado Falha(ErroAplicacao erro) => new(false, erro);

    public static Resultado Validacao(string mensagem, IReadOnlyList<string>? detalhes = null)
        => Falha(new ErroAplicacao(TipoErro.Validacao, mensagem, detalhes));

    public static Resultado NaoEncontrado(string mensagem)
        => Falha(new ErroAplicacao(TipoErro.NaoEncontrado, mensagem));

    public static Resultado Conflito(string mensagem)
        => Falha(new ErroAplicacao(TipoErro.Conflito, mensagem));

    public static Resultado NaoAutorizado(string mensagem)
        => Falha(new ErroAplicacao(TipoErro.NaoAutorizado, mensagem));
}

public sealed class Resultado<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public ErroAplicacao? Erro { get; }

    private Resultado(bool sucesso, T? valor, ErroAplicacao? erro)
    {
        Sucesso = sucesso;
        Valor = valor;
        Erro = erro;
    }

    public static Resultado<T> Ok(T valor) => new(true, valor, null);

    public static Resultado<T> Falha(ErroAplicacao erro) => new(false, default, erro);

    public static Resultado<T> Validacao(string mensagem, IReadOnlyList<string>? detalhes = null)
        => Falha(new ErroAplicacao(TipoErro.Validacao, mensagem, detalhes));

    public static Resultado<T> NaoEncontrado(string mensagem)
        => Falha(new ErroAplicacao(TipoErro.NaoEncontrado, mensagem));

    public static Resultado<T> Conflito(string mensagem)
        => Falha(new ErroAplicacao(TipoErro.Conflito, mensagem));

    public static Resultado<T> NaoAutorizado(string mensagem)
        => Falha(new ErroAplicacao(TipoErro.NaoAutorizado, mensagem));
}
