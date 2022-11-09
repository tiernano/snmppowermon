FROM mcr.microsoft.com/dotnet/nightly/sdk:7.0-jammy AS build-env

LABEL org.opencontainers.image.source  "https://github.com/tiernano/snmppowermon"
LABEL org.opencontainers.image.description "SNMP Power Monitor, used for HomeAssistant"
LABEL org.opencontainers.image.url "https://github.com/tiernano/snmppowermon"

WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0-jammy-chiseled
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "snmppowermon.dll"]