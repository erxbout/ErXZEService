#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["MqttServerExtension/MqttServerExtension.csproj", "MqttServerExtension/"]
RUN dotnet restore "MqttServerExtension/MqttServerExtension.csproj"
COPY . .
WORKDIR "/src/MqttServerExtension"
RUN dotnet build "MqttServerExtension.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MqttServerExtension.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MqttServerExtension.dll"]