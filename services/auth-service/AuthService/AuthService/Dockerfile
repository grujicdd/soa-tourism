FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy proto files
COPY protos/ /src/protos/

# Copy csproj and restore
COPY services/auth-service/AuthService/AuthService/AuthService.csproj services/auth-service/
WORKDIR /src/services/auth-service
RUN dotnet restore

# Copy source code
COPY services/auth-service/AuthService/AuthService .
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "AuthService.dll"]