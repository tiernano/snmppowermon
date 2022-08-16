FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0-jammy AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0-jammy-chiseled
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "snmppowermon.dll"]