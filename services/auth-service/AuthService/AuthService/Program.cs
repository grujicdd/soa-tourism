using AuthService.Models;
using AuthService.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
    ?? throw new Exception("DatabaseSettings not found in configuration");

var mongoClient = new MongoClient(databaseSettings.ConnectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseSettings.DatabaseName);
var usersCollection = mongoDatabase.GetCollection<User>(databaseSettings.UsersCollectionName);

builder.Services.AddSingleton(usersCollection);

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new Exception("JwtSettings not found in configuration");

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<TokenService>();

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

// Map gRPC service
app.MapGrpcService<AuthGrpcService>();

// Health check endpoint
app.MapGet("/", () => "Auth Service is running. Use a gRPC client to communicate.");

app.Run();