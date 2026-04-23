FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MotorCare.Domain/MotorCare.Domain.csproj MotorCare.Domain/
COPY MotorCare.Application/MotorCare.Application.csproj MotorCare.Application/
COPY MotorCare.Infrastructure/MotorCare.Infrastructure.csproj MotorCare.Infrastructure/
COPY MotorCare.Api/MotorCare.Api.csproj MotorCare.Api/
RUN dotnet restore MotorCare.Api/MotorCare.Api.csproj

COPY . .
RUN dotnet build MotorCare.Api/MotorCare.Api.csproj -c Release -o /app/build --no-restore /p:TreatWarningsAsErrors=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DataProtection__KeysPath=/var/lib/motorcare/dataprotection

COPY --from=build /app/build ./

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 \
  CMD curl -fsS http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "MotorCare.Api.dll"]
