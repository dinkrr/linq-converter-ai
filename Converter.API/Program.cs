using Converter.API.Services;
using Converter.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Configure Vertex AI settings
builder.Services.Configure<VertexAIConfiguration>(
    builder.Configuration.GetSection("VertexAI"));

// Register HTTP client for Vertex AI service
builder.Services.AddHttpClient<IVertexAIService, VertexAIService>();

// Register services
builder.Services.AddScoped<IConfigurationValidationService, ConfigurationValidationService>();
builder.Services.AddScoped<IVertexAIService, VertexAIService>();
builder.Services.AddScoped<ICodeConversionService, CodeConversionService>();

// Configure CORS to allow Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Validate configuration at startup
using (var scope = app.Services.CreateScope())
{
    var configValidator = scope.ServiceProvider.GetRequiredService<IConfigurationValidationService>();
    var (isValid, errorMessage) = configValidator.ValidateVertexAIConfiguration();

    if (!isValid)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError("Vertex AI configuration validation failed: {ErrorMessage}.", errorMessage);
        logger.LogError("Application cannot start without proper Vertex AI configuration. Please update your configuration in appsettings.json");
        throw new InvalidOperationException($"Invalid Vertex AI configuration: {errorMessage}");
    }
    else
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Vertex AI configuration validated successfully. Custom agent system ready.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expression Converter API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
