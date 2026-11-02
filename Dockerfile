# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the dependency layer is cached between code changes.
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY SignalRHub.Domain/SignalRHub.Domain.csproj SignalRHub.Domain/
COPY SignalRHub.Application/SignalRHub.Application.csproj SignalRHub.Application/
COPY SignalRHub.Infrastructure/SignalRHub.Infrastructure.csproj SignalRHub.Infrastructure/
COPY SignalRHub.App/SignalRHub.App.csproj SignalRHub.App/
RUN dotnet restore SignalRHub.App/SignalRHub.App.csproj

COPY SignalRHub.Domain/ SignalRHub.Domain/
COPY SignalRHub.Application/ SignalRHub.Application/
COPY SignalRHub.Infrastructure/ SignalRHub.Infrastructure/
COPY SignalRHub.App/ SignalRHub.App/
RUN dotnet publish SignalRHub.App/SignalRHub.App.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .

# curl is used by the Docker healthcheck (GET /health).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

USER $APP_UID
ENTRYPOINT ["dotnet", "SignalRHub.App.dll"]
