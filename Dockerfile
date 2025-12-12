# Base runtime (do uruchamiania)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

# SDK (do budowania aplikacji)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["app/SOM2_2.csproj", "app/"]
RUN dotnet restore "app/SOM2_2.csproj"
COPY ./app ./app
WORKDIR "/src/app"
RUN dotnet build "SOM2_2.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "SOM2_2.csproj" -c Release -o /app/publish

# Finalny obraz
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SOM2_2.dll"]
