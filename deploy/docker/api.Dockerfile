FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/DigitalSignature.Domain/DigitalSignature.Domain.csproj src/DigitalSignature.Domain/
COPY src/DigitalSignature.Application/DigitalSignature.Application.csproj src/DigitalSignature.Application/
COPY src/DigitalSignature.Infrastructure/DigitalSignature.Infrastructure.csproj src/DigitalSignature.Infrastructure/
COPY src/DigitalSignature.Api/DigitalSignature.Api.csproj src/DigitalSignature.Api/

RUN dotnet restore src/DigitalSignature.Api/DigitalSignature.Api.csproj

COPY src/ src/
RUN dotnet publish src/DigitalSignature.Api/DigitalSignature.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "DigitalSignature.Api.dll"]
