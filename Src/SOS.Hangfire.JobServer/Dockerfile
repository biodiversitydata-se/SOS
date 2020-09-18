#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Src/SOS.Hangfire.JobServer/SOS.Hangfire.JobServer.csproj", "Src/SOS.Hangfire.JobServer/"]
COPY ["Src/SOS.lib/SOS.Lib.csproj", "Src/SOS.lib/"]
COPY ["Src/SOS.Export/SOS.Export.csproj", "Src/SOS.Export/"]
COPY ["Src/SOS.Import/SOS.Import.csproj", "Src/SOS.Import/"]
COPY ["Src/External/DwC-A_dotnet/DwC-A_dotnet.csproj", "Src/External/DwC-A_dotnet/"]
COPY ["Src/SOS.Process/SOS.Process.csproj", "Src/SOS.Process/"]
RUN dotnet restore "Src/SOS.Hangfire.JobServer/SOS.Hangfire.JobServer.csproj"
COPY . .
WORKDIR "/src/Src/SOS.Hangfire.JobServer"
RUN dotnet build "SOS.Hangfire.JobServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SOS.Hangfire.JobServer.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT="dev"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SOS.Hangfire.JobServer.dll"]