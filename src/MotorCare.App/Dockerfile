FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MotorCare.App/MotorCare.App.csproj MotorCare.App/
RUN dotnet restore MotorCare.App/MotorCare.App.csproj

COPY . .
RUN dotnet publish MotorCare.App/MotorCare.App.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DataProtection__KeysPath=/var/lib/motorcare/dataprotection

COPY --from=build /app/publish ./

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 \
  CMD curl -fsS http://127.0.0.1:8080/ || exit 1

ENTRYPOINT ["dotnet", "MotorCare.App.dll"]
