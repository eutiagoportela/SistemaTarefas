FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/SistemaTarefas.Domain/SistemaTarefas.Domain.csproj src/SistemaTarefas.Domain/
COPY src/SistemaTarefas.Communication/SistemaTarefas.Communication.csproj src/SistemaTarefas.Communication/
COPY src/SistemaTarefas.Application/SistemaTarefas.Application.csproj src/SistemaTarefas.Application/
COPY src/SistemaTarefas.Infrastructure/SistemaTarefas.Infrastructure.csproj src/SistemaTarefas.Infrastructure/
COPY src/SistemaTarefas.Api/SistemaTarefas.Api.csproj src/SistemaTarefas.Api/
RUN dotnet restore src/SistemaTarefas.Api/SistemaTarefas.Api.csproj
COPY src/ src/
RUN dotnet publish src/SistemaTarefas.Api/SistemaTarefas.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser
COPY --from=build /app .
COPY docker-entrypoint.sh .
RUN sed -i 's/\r$//' docker-entrypoint.sh && chmod +x docker-entrypoint.sh
USER appuser
EXPOSE 8080
ENTRYPOINT ["./docker-entrypoint.sh"]
