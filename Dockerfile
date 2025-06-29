# Dockerfile to build and pack Zentient.Results supporting .NET 6, 8, 9

# Use the stable .NET 9.0 SDK image.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set environment variables to prevent telemetry and logo output
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_NOLOGO=true

# Install GitVersion.Tool globally
RUN dotnet tool install --global GitVersion.Tool

# Install jq for JSON processing
RUN apt-get update && \
    apt-get install -y jq && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory inside the container to /app.
WORKDIR /app

# Copy the entire repository content into the /app directory in the container.
COPY . .

# --- START DEBUGGING STEPS INSIDE DOCKER ---
# List contents of the /app directory (where your repo is copied)
RUN echo "--- Contents of /app (after COPY) ---" && ls -aF /app
RUN echo "--- Contents of /app/src (after COPY) ---" && ls -aF /app/src
RUN echo "--- Contents of /app/tests (after COPY) ---" && ls -aF /app/tests
# Display the content of the solution file as seen by Docker
RUN echo "--- Contents of Zentient.Results.sln (inside Docker) ---" && cat Zentient.Results.sln
# --- END DEBUGGING STEPS INSIDE DOCKER ---

# Restore dependencies for the solution.
RUN dotnet restore Zentient.Results.sln

# Build the solution in Release configuration.
RUN dotnet build Zentient.Results.sln -c Release

# Run tests for the solution.
RUN dotnet test Zentient.Results.sln --no-build --configuration Release

# Pack the main NuGet package.
RUN dotnet pack src/Results/Zentient.Results.csproj -c Release -o /artifacts --no-build

# Final stage (optional, for local inspection): copy artifacts out
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final

WORKDIR /app
COPY --from=build /artifacts ./

# Default command to list the built artifacts for verification
CMD ["ls", "-l", "/app"]
