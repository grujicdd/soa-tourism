using BlogService.Models;
using BlogService.Neo4jClient;
using BlogService.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
    ?? throw new Exception("DatabaseSettings not found in configuration");

var mongoClient = new MongoClient(databaseSettings.ConnectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseSettings.DatabaseName);

var blogsCollection = mongoDatabase.GetCollection<Blog>(databaseSettings.BlogsCollectionName);
var commentsCollection = mongoDatabase.GetCollection<Comment>(databaseSettings.CommentsCollectionName);
var likesCollection = mongoDatabase.GetCollection<Like>(databaseSettings.LikesCollectionName);

builder.Services.AddSingleton(blogsCollection);
builder.Services.AddSingleton(commentsCollection);
builder.Services.AddSingleton(likesCollection);

// Configure Neo4j
var neo4jSettings = builder.Configuration.GetSection("Neo4jSettings").Get<Neo4jSettings>()
    ?? throw new Exception("Neo4jSettings not found in configuration");

builder.Services.AddSingleton(neo4jSettings);
builder.Services.AddSingleton<FollowerService>();

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

// Map gRPC service
app.MapGrpcService<BlogGrpcService>();

// Health check endpoint
app.MapGet("/", () => "Blog Service is running. Use a gRPC client to communicate.");

app.Run();
