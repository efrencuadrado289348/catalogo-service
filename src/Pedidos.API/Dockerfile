
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY src/Pedidos.Domain/Pedidos.Domain.csproj src/Pedidos.Domain/
COPY src/Pedidos.Application/Pedidos.Application.csproj src/Pedidos.Application/
COPY src/Pedidos.Infrastructure/Pedidos.Infrastructure.csproj src/Pedidos.Infrastructure/
COPY src/Pedidos.API/Pedidos.API.csproj src/Pedidos.API/

RUN dotnet restore src/Pedidos.API/Pedidos.API.csproj

COPY src/Pedidos.Domain/ src/Pedidos.Domain/
COPY src/Pedidos.Application/ src/Pedidos.Application/
COPY src/Pedidos.Infrastructure/ src/Pedidos.Infrastructure/
COPY src/Pedidos.API/ src/Pedidos.API/

RUN dotnet publish src/Pedidos.API/Pedidos.API.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Pedidos.API.dll"]
