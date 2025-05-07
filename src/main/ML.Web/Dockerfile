FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG API_BASE_URL=http://api

COPY ["src/main/ML.Web/ML.Web.csproj", "src/main/ML.Web/"]
RUN dotnet restore "./src/main/ML.Web/ML.Web.csproj"

COPY . .

RUN apt-get update && apt-get install -y jq
RUN jq ".ApiSettings.BaseUrl = \"$API_BASE_URL\"" src/main/ML.Web/wwwroot/appsettings.json > /tmp/appsettings.json && \
    mv /tmp/appsettings.json src/main/ML.Web/wwwroot/appsettings.json

RUN dotnet publish "src/main/ML.Web/ML.Web.csproj" -c Release -o /app/publish


FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY --from=build /app/publish/wwwroot .

EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]