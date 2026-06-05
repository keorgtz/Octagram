FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OctagramDelivery.Domain/OctagramDelivery.Domain.csproj", "OctagramDelivery.Domain/"]
COPY ["OctagramDelivery.Application/OctagramDelivery.Application.csproj", "OctagramDelivery.Application/"]
COPY ["OctagramDelivery.Infrastructure/OctagramDelivery.Infrastructure.csproj", "OctagramDelivery.Infrastructure/"]
COPY ["OctagramDelivery.Api/OctagramDelivery.Api.csproj", "OctagramDelivery.Api/"]
RUN dotnet restore "OctagramDelivery.Api/OctagramDelivery.Api.csproj"
COPY . .
WORKDIR "/src/OctagramDelivery.Api"
RUN dotnet publish "OctagramDelivery.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OctagramDelivery.Api.dll"]
