# How to create a Blazor Web App (.NET9) with an AI ChatBot and Ollama phi3:latest model

## 1. Donwload and Install AI Ollama in your laptop

**https://ollama.com/download**

![image](https://github.com/user-attachments/assets/b89a70a5-a5ff-4f81-83e4-ea352d7d1fd9)

To verify the Ollama installation run these commands:

For downloading the phi3 model:

```
ollama run phi3
```

For listing the models:

```
ollama list
```

![image](https://github.com/user-attachments/assets/caf096b3-2d59-4534-b943-6685e3f1f3ef)

Verify Ollama is running

```
curl http://localhost:11434
```

![image](https://github.com/user-attachments/assets/f54ed356-d5b5-4e97-8652-e5abee40df6b)

Send a request:

```
curl -X POST http://localhost:11434/v1/completions ^
-H "Content-Type: application/json" ^
-d "{ \"model\": \"phi3:latest\", \"prompt\": \"hello\" }"
```

![image](https://github.com/user-attachments/assets/fd1abcd5-967a-44f8-adc4-82ed4d784783)

## 2. Create a Blazor Web App (.NET 9)

We run Visual Studio 2022 Community Edition and we Create a new Project

![image](https://github.com/user-attachments/assets/05a493e4-b72c-4158-b9d9-0a99ac210d12)

We select the Blazor Web App project template

![image](https://github.com/user-attachments/assets/c5c5f456-432e-42c9-bdaf-1539abd4fdcf)

We input the project name and location

![image](https://github.com/user-attachments/assets/6f26be1e-e554-41fe-8ec4-c4b8d0706af8)

We select the .NET 9 framework and leave the other options with the default values, and we press the Create button

![image](https://github.com/user-attachments/assets/43e7f31a-9505-43f1-b6c7-92368cae8b1f)

We verify the project folders and files structure

![image](https://github.com/user-attachments/assets/f7add044-2822-48a6-814b-b1a796eebee2)

## 3. Load the Nuget Packages

![image](https://github.com/user-attachments/assets/57c9b3f5-aa6f-4127-adb7-5329be217e50)

## 4. Modify the middleware(Program.cs)

We first have to register the **Log Service**

```csharp
builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});
```

Then we register the **Chat Client for Ollama Service**

```csharp
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
```

We also have to register the default **Chat Messages Service**

```csharp
builder.Services.AddSingleton<List<ChatMessage>>(static serviceProvider =>
{
    return new List<ChatMessage>()
    {
        new ChatMessage(ChatRole.System, "You are a useful assistant that replies using short and precise sentences.")
    };
});
```

We verify the whole **Program.cs** file

```csharp
using BlazorAIChatBotOllama.Components;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
```

## 5. Add the Chatbot

We create a new folder **Chatbot** inside the **Components** folder

![image](https://github.com/user-attachments/assets/24f74f8c-eb4d-46f9-9307-85a4dfab7324)

Then we are going to create the classes files and razor components

We first create the **ChatState.cs**. This file can be summarized in two lines:

```csharp
ChatMessages.Add(new ChatMessage(ChatRole.User, userText));
...
ChatMessages.Add(new ChatMessage(ChatRole.Assistant, $"My apologies, but I encountered an unexpected error.\n\n<p style=\"color: red\">{e}</p>"));
```

This is the whole **ChatState.cs** file:

```csharp
using Microsoft.Extensions.AI;
using System.Security.Claims;
using System.Text;

namespace AspireApp.WebApp.Chatbot;

public class ChatState
{
    private readonly ILogger _logger;
    private readonly IChatClient _chatClient;
    private List<ChatMessage> _chatMessages;

    public List<ChatMessage> ChatMessages { get => _chatMessages; set => _chatMessages = value; }

    public ChatState(ClaimsPrincipal user, IChatClient chatClient, List<ChatMessage> chatMessages, ILogger logger)
    {
        _logger = logger;
        _chatClient = chatClient;
        ChatMessages = chatMessages;
    }

    public async Task AddUserMessageAsync(string userText, Action onMessageAdded)
    {
        ChatMessages.Add(new ChatMessage(ChatRole.User, userText));
        onMessageAdded();

        try
        {
            _logger.LogInformation("Sending message to chat client.");
            _logger.LogInformation($"user Text: {userText}");

            var result = await _chatClient.CompleteAsync(ChatMessages);
            ChatMessages.Add(new ChatMessage(ChatRole.Assistant, result.Message.Text));
            
            _logger.LogInformation($"Assistant Response: {result.Message.Text}");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error getting chat completions.");
            }

            // format the exception using HTML to show the exception details in a chat panel as response
            ChatMessages.Add(new ChatMessage(ChatRole.Assistant, $"My apologies, but I encountered an unexpected error.\n\n<p style=\"color: red\">{e}</p>"));
        }
        onMessageAdded();
    }
}
```

We also have to create the **MessageProcessor.cs** file 

```csharp
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace AspireApp.WebApp.Chatbot;

public static partial class MessageProcessor
{
    public static MarkupString AllowImages(string message)
    {
        // Having to process markdown and deal with HTML encoding isn't ideal. If the language model could return
        // search results in some defined format like JSON we could simply loop over it in .razor code. This is
        // fine for now though.

        var result = new StringBuilder();
        var prevEnd = 0;
        message = message.Replace("&lt;", "<").Replace("&gt;", ">");

        foreach (Match match in FindMarkdownImages().Matches(message))
        {
            var contentToHere = message.Substring(prevEnd, match.Index - prevEnd);
            result.Append(HtmlEncoder.Default.Encode(contentToHere));
            result.Append($"<img title=\"{(HtmlEncoder.Default.Encode(match.Groups[1].Value))}\" src=\"{(HtmlEncoder.Default.Encode(match.Groups[2].Value))}\" />");

            prevEnd = match.Index + match.Length;
        }
        result.Append(HtmlEncoder.Default.Encode(message.Substring(prevEnd)));

        return new MarkupString(result.ToString());
    }

    public static MarkupString ProcessMessageToHTML(string message)
    {
        return new MarkupString(message);
    }

    [GeneratedRegex(@"\!?\[([^\]]+)\]\s*\(([^\)]+)\)")]
    private static partial Regex FindMarkdownImages();
}
```

Now we create the razor components:

We create the ShowChatbot button

**ShowChatbotButton.razor**

```razor
@inject NavigationManager Nav

<a class="show-chatbot" href="@Nav.GetUriWithQueryParameter("chat", true)" title="Show chatbot"></a>

@if (ShowChat)
{
    <Chatbot />
}

@code {
    [SupplyParameterFromQuery(Name = "chat")]
    public bool ShowChat { get; set; }
}
```

And also we create the Chatbot razor component

**Chatbot.razor**

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
@using Microsoft.AspNetCore.Components.Authorization
@using AspireApp.WebApp.Chatbot
@using Microsoft.Extensions.AI
@inject IJSRuntime JS
@inject NavigationManager Nav

@inject AuthenticationStateProvider AuthenticationStateProvider
@inject ILogger Logger
@inject IConfiguration Configuration
@inject IServiceProvider ServiceProvider

<div class="floating-pane">
    <a href="@Nav.GetUriWithQueryParameter("chat", (string?)null)" class="hide-chatbot" title="Close Chat"><span>✖</span></a>

    <div class="chatbot-chat" @ref="chat">
        @if (chatState is not null)
        {
            foreach (var message in chatState.ChatMessages.Where(m => m.Role == ChatRole.Assistant || m.Role == ChatRole.User))
            {
                if (!string.IsNullOrEmpty(message.Contents[0].ToString()))
                {
                    <p @key="@message" class="message message-@message.Role">@MessageProcessor.AllowImages(message.Contents[0].ToString()!)</p>                    
                }
            }
        }
        else if (missingConfiguration)
        {
            <p class="message message-assistant"><strong>The chatbot is missing required configuration.</strong> Please review your app settings.</p>
        }

        @if (thinking)
        {
            <p class="thinking">"[phi3:latest]" is Thinking...</p>
        }

    </div>

    <form class="chatbot-input" @onsubmit="SendMessageAsync">
        <textarea placeholder="Start chatting..." @ref="@textbox" @bind="messageToSend"></textarea>
        <button type="submit" title="Send" disabled="@(chatState is null)">Send</button>
    </form>
</div>

@code {
    bool missingConfiguration;
    ChatState? chatState;
    ElementReference textbox;
    ElementReference chat;
    string? messageToSend;
    bool thinking;
    IJSObjectReference? jsModule;

    protected override async Task OnInitializedAsync()
    {
        IChatClient chatClient = ServiceProvider.GetService<IChatClient>();
        List<ChatMessage> chatMessages = ServiceProvider.GetService<List<ChatMessage>>();
        if (chatClient is not null)
        {
            AuthenticationState auth = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            chatState = new ChatState(auth.User, chatClient, chatMessages, Logger);
        }
        else
        {
            missingConfiguration = true;
        }
    }

    private async Task SendMessageAsync()
    {
        var messageCopy = messageToSend?.Trim();
        messageToSend = null;

        if (chatState is not null && !string.IsNullOrEmpty(messageCopy))
        {
            thinking = true;
            await chatState.AddUserMessageAsync(messageCopy, onMessageAdded: StateHasChanged);
            thinking = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Chatbot/Chatbot.razor.js");
        await jsModule.InvokeVoidAsync("scrollToEnd", chat);

        if (firstRender)
        {
            await textbox.FocusAsync();
            await jsModule.InvokeVoidAsync("submitOnEnter", textbox);
        }
    }
}
```

## 6. Modify the Home.razor component

We have to invoke the Show Chatbot button from the home page, for this purpose we add the following code:

**Home.razor**

```razor
@page "/"

@using BlazorAIChatBotOllama.Components.Chatbot

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

Welcome to your new app.

<ShowChatbotButton />

@code {

    string ollamaSLMName = "";
    string ollamaModelUrl = "";
    string ollamaModelUrlModelList = "";
    string ollamaUrl = "";
    string modelStatus = "";

    protected override void OnInitialized()
    {
        // Set model name directly based on your screenshot information
        ollamaSLMName = "phi3:latest";

        // URL to Ollama model running on the local machine on port 11434
        ollamaModelUrl = $@"http://localhost:11434/library/{ollamaSLMName}";

        // Base URL for the local Ollama instance on port 11434
        ollamaUrl = "http://localhost:11434";

        // Endpoint to retrieve model list from local Ollama instance
        ollamaModelUrlModelList = $@"{ollamaUrl}/api/tags";
    }
}
```

## 7. Run the application a see the results

![image](https://github.com/user-attachments/assets/b3d806c5-7771-4fe4-a67c-ce6fb8467142)

![image](https://github.com/user-attachments/assets/5266611a-413e-48b9-baa1-400526813590)

We write the message and press the Send button

![image](https://github.com/user-attachments/assets/97b84e9a-7ed8-4d63-89a6-aa772afbfaeb)


