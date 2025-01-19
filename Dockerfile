FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

COPY . /build
WORKDIR /build

RUN dotnet publish ./src/PallasBot.App.Bot -c Release -o /artifacts -p:ContinuousIntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0

COPY --from=build /artifacts /app

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV OTEL_SERVICE_NAME=pallas-bot

EXPOSE 80

ENTRYPOINT ["dotnet", "PallasBot.App.Bot.dll"]
