FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OctagramDelivery.Api/OctagramDelivery.Api.csproj", "OctagramDelivery.Api/"]
COPY ["OctagramDelivery.Shared/OctagramDelivery.Shared.csproj", "OctagramDelivery.Shared/"]
RUN dotnet restore "OctagramDelivery.Api/OctagramDelivery.Api.csproj"
COPY . .
WORKDIR "/src/OctagramDelivery.Api"
RUN dotnet build "OctagramDelivery.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OctagramDelivery.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OctagramDelivery.Api.dll"]
