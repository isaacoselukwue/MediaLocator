FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/main/ML.Service/ML.Service.csproj", "src/main/ML.Service/"]
COPY ["src/main/ML.Application/ML.Application.csproj", "src/main/ML.Application/"]
COPY ["src/main/ML.Domain/ML.Domain.csproj", "src/main/ML.Domain/"]
COPY ["src/main/ML.Infrastructure/ML.Infrastructure.csproj", "src/main/ML.Infrastructure/"]
RUN dotnet restore "./src/main/ML.Service/ML.Service.csproj"
COPY . .
WORKDIR "/src/src/main/ML.Service"
RUN dotnet build "./ML.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ML.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ML.Service.dll"]