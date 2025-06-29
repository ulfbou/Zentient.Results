# Dockerfile to build and pack Zentient.Results supporting .NET 6, 8, 9

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install additional SDKs for cross-targeting
RUN apt-get update \
    && apt-get install -y wget apt-transport-https software-properties-common \
    && wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-6.0 dotnet-sdk-8.0 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy everything except what's ignored via .dockerignore
COPY . .

# Restore dependencies
RUN dotnet restore Zentient.Results.sln

# Build and pack Zentient.Results
RUN dotnet build Zentient.Results.sln -c Release
RUN dotnet pack Src/Results/Zentient.Results.csproj -c Release -o /artifacts

# Final stage (optional): copy artifacts out
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final

WORKDIR /app
COPY --from=build /artifacts ./

# Default to listing the artifacts
CMD ["ls", "-l", "/app"]
