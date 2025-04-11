FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/tests/ML.AcceptanceTests/ML.AcceptanceTests.csproj", "src/tests/ML.AcceptanceTests/"]
COPY ["src/main/ML.Web/ML.Web.csproj", "src/main/ML.Web/"]

RUN dotnet restore "./src/tests/ML.AcceptanceTests/ML.AcceptanceTests.csproj"

COPY . .

RUN dotnet build "src/tests/ML.AcceptanceTests/ML.AcceptanceTests.csproj" -c Release

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        wget \
        ca-certificates \
        curl \
        unzip \
        libglib2.0-0 \
        libnss3 \
        libnspr4 \
        libatk1.0-0 \
        libatk-bridge2.0-0 \
        libcups2 \
        libdrm2 \
        libdbus-1-3 \
        libxkbcommon0 \
        libxcomposite1 \
        libxdamage1 \
        libxfixes3 \
        libxrandr2 \
        libgbm1 \
        libpango-1.0-0 \
        libcairo2 \
        libasound2 \
        libatspi2.0-0 \
        libx11-6 \
        xvfb \
        fonts-liberation \
        fonts-noto-color-emoji \
    && rm -rf /var/lib/apt/lists/*

RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="$PATH:/root/.dotnet/tools"

RUN /root/.dotnet/tools/playwright install --with-deps chromium

WORKDIR "/src/src/tests/ML.AcceptanceTests"

ENTRYPOINT ["xvfb-run", "--auto-servernum", "--server-args=-screen 0 1280x720x24", "dotnet", "test", "--no-restore", "--logger:trx", "--results-directory:/testresults"]

VOLUME ["/testresults"]