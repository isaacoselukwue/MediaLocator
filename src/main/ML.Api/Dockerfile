FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/main/ML.Api/ML.Api.csproj", "src/main/ML.Api/"]
COPY ["src/main/ML.Application/ML.Application.csproj", "src/main/ML.Application/"]
COPY ["src/main/ML.Domain/ML.Domain.csproj", "src/main/ML.Domain/"]
COPY ["src/main/ML.Infrastructure/ML.Infrastructure.csproj", "src/main/ML.Infrastructure/"]
RUN dotnet restore "./src/main/ML.Api/ML.Api.csproj"
COPY . .
WORKDIR "/src/src/main/ML.Api"
RUN dotnet build "./ML.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ML.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ML.Api.dll"]