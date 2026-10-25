# Build from the solution folder:
#   docker build -f WebApi.Dockerfile -t qrsanad-webapi .
# Secrets come from the environment, e.g. -e Identity__Jwt__SecretKey=... -e Persistence__Sql__DefaultConnection=...

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Presentation/Tawqi3i.QrSanad.WebApi/Tawqi3i.QrSanad.WebApi.csproj
RUN dotnet publish src/Presentation/Tawqi3i.QrSanad.WebApi/Tawqi3i.QrSanad.WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The image's built-in non-root user.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Tawqi3i.QrSanad.WebApi.dll"]
