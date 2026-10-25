using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaTarefas.Api.Extensoes;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.UseCases.Tarefas;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Enums;

namespace SistemaTarefas.Api.Controllers;

/// <summary>Gestão de tarefas (ToDo) — requer JWT.</summary>
[Authorize]
[ApiController]
[Route("api/v1/tarefas")]
public sealed class TarefasController : ControllerBase
{
    /// <summary>
    /// Cria uma tarefa. Header opcional Idempotency-Key. Resposta inclui ETag (versão).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Criar(
        [FromServices] ICriarTarefaUseCase useCase,
        [FromServices] IIdempotenciaStore idempotencia,
        [FromServices] IUsuarioAtual usuario,
        [FromBody] RequestCriarTarefa request,
        [FromHeader(Name = "Idempotency-Key")] string? chaveIdempotencia,
        CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
        if (!string.IsNullOrWhiteSpace(chaveIdempotencia))
        {
            var cache = await idempotencia.ObterRespostaAsync(usuario.Id, chaveIdempotencia, hash, cancellationToken);
            if (cache is not null)
            {
                return Content(cache, "application/json");
            }
        }

        var resultado = await useCase.Execute(request, cancellationToken);
        var resposta = resultado.ParaHttp(HttpContext, "Tarefa criada com sucesso", StatusCodes.Status201Created);
        if (resultado.Sucesso && resultado.Valor is not null)
        {
            Response.Headers.ETag = $"\"{resultado.Valor.Versao}\"";
            if (!string.IsNullOrWhiteSpace(chaveIdempotencia))
            {
                var json = JsonSerializer.Serialize(RespostaPadrao<TarefaResponse>.ComSucesso(resultado.Valor, "Tarefa criada com sucesso", HttpContext.TraceIdentifier));
                await idempotencia.SalvarAsync(usuario.Id, chaveIdempotencia, hash, json, cancellationToken);
            }
        }

        return resposta;
    }

    /// <summary>
    /// Lista tarefas com filtros, paginação e ordenação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(RespostaPadrao<RespostaPaginada<TarefaResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<RespostaPaginada<TarefaResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<RespostaPaginada<TarefaResponse>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<RespostaPaginada<TarefaResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Listar(
        [FromServices] IListarTarefasUseCase useCase,
        [FromQuery] StatusTarefa? status,
        [FromQuery] DateTime? vencimentoDe,
        [FromQuery] DateTime? vencimentoAte,
        [FromQuery] bool? atrasadas,
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        [FromQuery] string ordenarPor = "vencimento",
        [FromQuery] string direcao = "asc",
        CancellationToken cancellationToken = default)
    {
        var resultado = await useCase.Execute(
            new FiltroTarefas(status, vencimentoDe, vencimentoAte, atrasadas, busca, pagina, tamanho, ordenarPor, direcao),
            cancellationToken);
        return resultado.ParaHttp(HttpContext, "Tarefas listadas");
    }

    /// <summary>
    /// Obtém uma tarefa por ID. Resposta inclui header ETag (versão para If-Match).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Obter(
        [FromServices] IObterTarefaUseCase useCase,
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.Execute(id, cancellationToken);
        if (resultado.Sucesso && resultado.Valor is not null)
        {
            Response.Headers.ETag = $"\"{resultado.Valor.Versao}\"";
        }

        return resultado.ParaHttp(HttpContext, "Tarefa encontrada");
    }

    /// <summary>
    /// Atualiza título, descrição e vencimento. Requer header If-Match com a versão (ETag).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Atualizar(
        [FromServices] IAtualizarTarefaUseCase useCase,
        Guid id,
        [FromBody] RequestAtualizarTarefa request,
        CancellationToken cancellationToken)
    {
        if (!TentarLerVersao(out var versao, out var erro))
        {
            return erro!;
        }

        var resultado = await useCase.Execute(id, versao, request, cancellationToken);
        return resultado.ParaHttp(HttpContext, "Tarefa atualizada");
    }

    /// <summary>
    /// Inicia a tarefa (Pendente → Em andamento). Requer If-Match.
    /// </summary>
    [HttpPost("{id:guid}/iniciar")]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Iniciar([FromServices] IAlterarStatusTarefaUseCase useCase, Guid id, CancellationToken ct)
        => ExecutarComando(id, (v) => useCase.Iniciar(id, v, ct));

    /// <summary>
    /// Conclui a tarefa (Em andamento → Concluído). Requer If-Match.
    /// </summary>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Concluir([FromServices] IAlterarStatusTarefaUseCase useCase, Guid id, CancellationToken ct)
        => ExecutarComando(id, (v) => useCase.Concluir(id, v, ct));

    /// <summary>
    /// Reabre a tarefa (Concluído → Pendente). Requer If-Match.
    /// </summary>
    [HttpPost("{id:guid}/reabrir")]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(RespostaPadrao<TarefaResponse>), StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> Reabrir([FromServices] IAlterarStatusTarefaUseCase useCase, Guid id, CancellationToken ct)
        => ExecutarComando(id, (v) => useCase.Reabrir(id, v, ct));

    /// <summary>
    /// Exclui a tarefa (soft delete). Requer If-Match.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remover(
        [FromServices] IRemoverTarefaUseCase useCase,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TentarLerVersao(out var versao, out var erro))
        {
            return erro!;
        }

        var resultado = await useCase.Execute(id, versao, cancellationToken);
        return resultado.ParaHttp(HttpContext, "Tarefa excluída com sucesso");
    }

    /// <summary>
    /// Obtém o histórico de auditoria da tarefa.
    /// </summary>
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(RespostaPadrao<List<HistoricoResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaPadrao<List<HistoricoResponse>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespostaPadrao<List<HistoricoResponse>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespostaPadrao<List<HistoricoResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Historico(
        [FromServices] IObterHistoricoTarefaUseCase useCase,
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.Execute(id, cancellationToken);
        return resultado.ParaHttp(HttpContext, "Histórico da tarefa");
    }

    private async Task<IActionResult> ExecutarComando(Guid id, Func<uint, Task<Domain.Resultados.Resultado<TarefaResponse>>> comando)
    {
        if (!TentarLerVersao(out var versao, out var erro))
        {
            return erro!;
        }

        var resultado = await comando(versao);
        if (resultado.Sucesso && resultado.Valor is not null)
        {
            Response.Headers.ETag = $"\"{resultado.Valor.Versao}\"";
        }

        return resultado.ParaHttp(HttpContext, "Operação realizada com sucesso");
    }

    private bool TentarLerVersao(out uint versao, out IActionResult? erro)
    {
        versao = 0;
        erro = null;
        if (!Request.Headers.TryGetValue("If-Match", out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            erro = StatusCode(StatusCodes.Status428PreconditionRequired,
                RespostaPadrao<object>.ComErro("Informe o header If-Match com o ETag obtido no GET.", "Pré-condição obrigatória", HttpContext.TraceIdentifier));
            return false;
        }

        var valor = raw.ToString().Trim().Trim('"');
        if (!uint.TryParse(valor, out versao))
        {
            erro = BadRequest(RespostaPadrao<object>.ComErro("If-Match inválido.", "Erros de validação", HttpContext.TraceIdentifier));
            return false;
        }

        return true;
    }
}
