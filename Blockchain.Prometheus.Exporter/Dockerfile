FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Blockchain.Prometheus.Exporter.csproj", "Blockchain.Prometheus.Exporter/"]
RUN dotnet restore "Blockchain.Prometheus.Exporter/Blockchain.Prometheus.Exporter.csproj"
WORKDIR "/src/Blockchain.Prometheus.Exporter"
COPY . .
RUN dotnet build "Blockchain.Prometheus.Exporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blockchain.Prometheus.Exporter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blockchain.Prometheus.Exporter.dll"]
