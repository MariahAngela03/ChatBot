using Microsoft.AspNetCore.Cors;
using ChatbotAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for frontend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://your-frontend-domain.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Register custom services
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddHttpClient(); // For external API calls if needed

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();