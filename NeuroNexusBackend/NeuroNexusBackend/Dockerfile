# Debug/Regular: Linux base images (Debian)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
# Opcional: força a app a escutar nas portas expostas
ENV ASPNETCORE_URLS=http://+:8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NeuroNexusBackend/NeuroNexusBackend.csproj", "NeuroNexusBackend/"]
RUN dotnet restore "./NeuroNexusBackend/NeuroNexusBackend.csproj"
COPY . .
WORKDIR "/src/NeuroNexusBackend"
RUN dotnet build "./NeuroNexusBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NeuroNexusBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NeuroNexusBackend.dll"]
