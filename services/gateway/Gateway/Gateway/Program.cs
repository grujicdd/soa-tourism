using Gateway.GrpcClients;
using Gateway.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Service URLs
var serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>()
    ?? throw new Exception("ServiceSettings not found in configuration");

// Register gRPC clients as singletons
builder.Services.AddSingleton(new AuthGrpcClient(serviceSettings.AuthServiceUrl));
builder.Services.AddSingleton(new TourGrpcClient(serviceSettings.TourServiceUrl));
builder.Services.AddSingleton(new BlogGrpcClient(serviceSettings.BlogServiceUrl));

// Configure CORS (for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();