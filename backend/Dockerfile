FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia a solution
COPY Ubs.Monitoring.sln ./

# 2. Copia TODOS os csproj (isso é CRÍTICO)
COPY src/Ubs.Monitoring.Domain/Ubs.Monitoring.Domain.csproj src/Ubs.Monitoring.Domain/
COPY src/Ubs.Monitoring.Application/Ubs.Monitoring.Application.csproj src/Ubs.Monitoring.Application/
COPY src/Ubs.Monitoring.Infrastructure/Ubs.Monitoring.Infrastructure.csproj src/Ubs.Monitoring.Infrastructure/
COPY src/Ubs.Monitoring.Api/Ubs.Monitoring.Api.csproj src/Ubs.Monitoring.Api/

# 3. Restore agora é consistente
RUN dotnet restore

# 4. Copia o restante do código
COPY . .

# 5. Publish SEM --no-restore (mais seguro)
RUN dotnet publish src/Ubs.Monitoring.Api/Ubs.Monitoring.Api.csproj \
    -c Release \
    -o /app/publish

# =========================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Ubs.Monitoring.Api.dll"]
