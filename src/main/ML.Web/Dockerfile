FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/main/ML.Web/ML.Web.csproj", "src/main/ML.Web/"]
RUN dotnet restore "./src/main/ML.Web/ML.Web.csproj"

COPY . .
RUN dotnet publish "src/main/ML.Web/ML.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY --from=build /app/publish/wwwroot .

EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]