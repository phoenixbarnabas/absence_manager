# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/absence_manager_backend/absence_manager_backend/absence_manager_backend.csproj", "absence_manager_backend/"]
COPY ["backend/absence_manager_backend/Data/Data.csproj", "Data/"]
COPY ["backend/absence_manager_backend/Entities/Entities.csproj", "Entities/"]
COPY ["backend/absence_manager_backend/Logic/Logic.csproj", "Logic/"]
RUN dotnet restore "backend/absence_manager_backend/absence_manager_backend/absence_manager_backend.csproj"
COPY . .
WORKDIR "/src/backend/absence_manager_backend/absence_manager_backend"
RUN dotnet build "absence_manager_backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "absence_manager_backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "absence_manager_backend.dll"]