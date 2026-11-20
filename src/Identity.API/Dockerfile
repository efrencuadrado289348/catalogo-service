FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Identity.API/Identity.API.csproj src/Identity.API/
RUN dotnet restore src/Identity.API/Identity.API.csproj

COPY src/Identity.API/ src/Identity.API/
RUN dotnet publish src/Identity.API/Identity.API.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Identity.API.dll"]
