using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS: дазволіць запыты з http://localhost:4200
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger / OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PhotoLib API",
        Version = "v1",
        Description = "Simple backend for PhotoLibApp v2"
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowLocalhost4200");

// Enable middleware only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhotoLib API V1");
        // c.RoutePrefix = string.Empty; // калі хочацца адкрываць Swagger на / 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
