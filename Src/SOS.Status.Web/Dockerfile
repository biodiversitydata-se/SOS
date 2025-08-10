# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5006

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install wasm-tools and Python
RUN dotnet workload install wasm-tools \
    && apt-get update \
    && apt-get install -y python3 python3-pip \
    && ln -s /usr/bin/python3 /usr/bin/python

COPY ["Directory.Build.props", "."]
COPY ["Src/SOS.Status.Web/SOS.Status.Web.csproj", "Src/SOS.Status.Web/"]
COPY ["Src/SOS.Shared.Api/SOS.Shared.Api.csproj", "Src/SOS.Shared.Api/"]
COPY ["Src/SOS.Lib/SOS.Lib.csproj", "Src/SOS.Lib/"]
COPY ["Src/SOS.Status.Web.Client/SOS.Status.Web.Client.csproj", "Src/SOS.Status.Web.Client/"]
RUN dotnet restore "./Src/SOS.Status.Web/SOS.Status.Web.csproj"

COPY . .
WORKDIR "/src/Src/SOS.Status.Web"
RUN dotnet build "./SOS.Status.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SOS.Status.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:BlazorWebAssemblyEnableLinking=true \
    /p:BlazorWebAssemblyEnableTimeZoneSupport=true \
    /p:RunAOTCompilation=false  # sätt true om du vill köra AOT också

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SOS.Status.Web.dll"]
