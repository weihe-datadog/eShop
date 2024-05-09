# Use the official image as a parent image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ARG DD_TRACER
RUN apt-get update && apt-get install -y curl
RUN mkdir -p /var/log/datadog/dotnet
RUN mkdir -p /opt/datadog
RUN curl -L $DD_TRACER | tar xzf - -C /opt/datadog

# Use the SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MvcShopApp.csproj", "./"]
RUN dotnet restore "MvcShopApp.csproj" 
COPY . .
RUN dotnet build "MvcShopApp.csproj" -a arm64 -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MvcShopApp.csproj" -a arm64 -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MvcShopApp.dll"]
