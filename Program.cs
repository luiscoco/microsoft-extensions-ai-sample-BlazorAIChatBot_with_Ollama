using BlazorAIChatBotOllama.Components;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add logging with detailed information
//builder.Services.AddLogging();
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.AddEventSourceLogger();
//builder.Logging.AddFilter("Microsoft", LogLevel.Information);
//builder.Logging.AddFilter("System", LogLevel.Information);
//builder.Logging.AddFilter("BlazorChatBot", LogLevel.Debug);

builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});

// Register the chat client for Ollama
builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger>();

    // Set up the Ollama connection string and default model
    var ollamaCnnString = "http://localhost:11434";
    var defaultLLM = "phi3:latest"; 

    logger.LogInformation("Ollama connection string: {0}", ollamaCnnString);
    logger.LogInformation("Default LLM: {0}", defaultLLM);

    // Create the Ollama chat client with the updated connection URI and model name
    IChatClient chatClient = new OllamaChatClient(new Uri(ollamaCnnString), defaultLLM);

    return chatClient;
});

// Register default chat messages
builder.Services.AddSingleton<List<ChatMessage>>(static serviceProvider =>
{
    return new List<ChatMessage>()
    {
        new ChatMessage(ChatRole.System, "You are a useful assistant that replies using short and precise sentences.")
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
