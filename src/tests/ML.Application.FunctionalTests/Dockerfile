FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/tests/ML.Application.FunctionalTests/ML.Application.FunctionalTests.csproj", "src/tests/ML.Application.FunctionalTests/"]
COPY ["src/main/ML.Application/ML.Application.csproj", "src/main/ML.Application/"]
COPY ["src/main/ML.Domain/ML.Domain.csproj", "src/main/ML.Domain/"]
COPY ["src/main/ML.Infrastructure/ML.Infrastructure.csproj", "src/main/ML.Infrastructure/"]

RUN dotnet restore "./src/tests/ML.Application.FunctionalTests/ML.Application.FunctionalTests.csproj"

COPY . .

WORKDIR "/src/src/tests/ML.Application.FunctionalTests"

ENTRYPOINT ["dotnet", "test", "--no-restore", "--logger:trx", "--results-directory:/testresults"]

VOLUME ["/testresults"]