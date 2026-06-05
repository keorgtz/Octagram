FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OctagramDelivery.Client/OctagramDelivery.Client.csproj", "OctagramDelivery.Client/"]
COPY ["OctagramDelivery.Shared/OctagramDelivery.Shared.csproj", "OctagramDelivery.Shared/"]
RUN dotnet restore "OctagramDelivery.Client/OctagramDelivery.Client.csproj"
COPY . .
WORKDIR "/src/OctagramDelivery.Client"
RUN dotnet publish "OctagramDelivery.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY OctagramDelivery.Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
