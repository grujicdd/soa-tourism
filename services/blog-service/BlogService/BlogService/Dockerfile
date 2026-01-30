FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy proto files
COPY protos/ /src/protos/

# Copy csproj and restore
COPY services/blog-service/BlogService/BlogService/BlogService.csproj services/blog-service/
WORKDIR /src/services/blog-service
RUN dotnet restore

# Copy source code
COPY services/blog-service/BlogService/BlogService .
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5002
ENTRYPOINT ["dotnet", "BlogService.dll"]