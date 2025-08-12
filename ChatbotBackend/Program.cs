using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add the bot service
builder.Services.AddSingleton<ChatBotService>();

var app = builder.Build();

app.UseCors();
app.UseStaticFiles(); // if serving frontend from wwwroot

app.MapPost("/chat", async (ChatRequest req, ChatBotService bot) => {
    var reply = bot.GetReply(req.Message ?? string.Empty);
    return Results.Json(new { reply = reply });
});

app.MapGet("/health", () => Results.Ok(new { status = "up" }));

app.Run();

// Models
public record ChatRequest(string Message);
