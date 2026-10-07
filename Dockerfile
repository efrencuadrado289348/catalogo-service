FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Catalog.Domain/Catalog.Domain.csproj src/Catalog.Domain/
COPY src/Catalog.Application/Catalog.Application.csproj src/Catalog.Application/
COPY src/Catalog.Infrastructure/Catalog.Infrastructure.csproj src/Catalog.Infrastructure/
COPY src/Catalog.API/Catalog.API.csproj src/Catalog.API/

RUN dotnet restore src/Catalog.API/Catalog.API.csproj

COPY src/ src/
RUN dotnet publish src/Catalog.API/Catalog.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Catalog.API.dll"]
