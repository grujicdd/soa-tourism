FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy proto files
COPY protos/ /src/protos/

# Copy csproj and restore
COPY services/gateway/Gateway/Gateway/Gateway.csproj services/gateway/
WORKDIR /src/services/gateway
RUN dotnet restore

# Copy source code
COPY services/gateway/Gateway/Gateway .
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Gateway.dll"]