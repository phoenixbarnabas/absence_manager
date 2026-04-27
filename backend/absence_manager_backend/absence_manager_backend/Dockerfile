FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["backend/absence_manager_backend/absence_manager_backend/absence_manager_backend.csproj", "backend/absence_manager_backend/absence_manager_backend/"]
COPY ["backend/absence_manager_backend/Data/Data.csproj", "backend/absence_manager_backend/Data/"]
COPY ["backend/absence_manager_backend/Entities/Entities.csproj", "backend/absence_manager_backend/Entities/"]
COPY ["backend/absence_manager_backend/Logic/Logic.csproj", "backend/absence_manager_backend/Logic/"]

RUN dotnet restore "backend/absence_manager_backend/absence_manager_backend/absence_manager_backend.csproj"

COPY . .

WORKDIR "/src/backend/absence_manager_backend/absence_manager_backend"
RUN dotnet build "absence_manager_backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "absence_manager_backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "absence_manager_backend.dll"]
