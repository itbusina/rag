using console.Data;
using console.Embeddings;
using console.Models;
using console.Retrieving;
using console.Summarization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();

var storage = new Dictionary<string, List<Chunk>>();

var embedder = new OllamaEmbedder("nomic-embed-text"); // new OpenAIEmbedder("text-embedding-3-small", apiKey);
var summarizer = new OllamaSummarizer("llama3.1:8b"); //new OpenAISummarizer("gpt-4.1-mini", apiKey);
var retriever = new Retriever(embedder);

app.MapPost("/assistant", async (AssistantRequest request) =>
{
    IDataLoader dataLoader = request.SourceType switch
    {
        "file" => new FileDataLoader(embedder, request.SourceValue),
        "qa" => new QADataLoader(embedder, request.SourceValue),
        "github" => new GitHubDataLoader(embedder, request.SourceValue), // Optional: Set GITHUB_TOKEN environment variable for higher API rate limits
        "http" => new HttpDataLoader(embedder, request.SourceValue),
        "sitemap" => new SitemapDataLoader(embedder, request.SourceValue),
        _ => throw new InvalidOperationException("Unsupported data source. Use 'file', 'qa', 'github', 'http', or 'sitemap'."),
    };

    // Step 1. Load file content
    await dataLoader.LoadAsync();

    // Step 2: Load chunks for data source
    var chunks = await dataLoader.GetContentChunks();

    // Store chunks in memory (in a real app, consider using a persistent storage)
    var id = Guid.NewGuid().ToString();
    storage[id] = chunks;

    return Results.Ok(new
    {
        Id = id,
        Chunks = chunks.Select(x => new { x.Content, x.Metadata }),
    });
})
.WithName("CreateAssistant");

app.MapPost("/assistant/{id:guid}", async (Guid id, [FromBody] string query) =>
{
    if (storage.TryGetValue(id.ToString(), out var chunks))
    {
        query = query.Trim();

        if (string.IsNullOrEmpty(query))
        {
            return Results.BadRequest("Query cannot be empty.");
        }

        // Step 5. Retrieve top k findings
        var topChunks = await retriever.GetTopKChunks(chunks, query, k: 3);

        // Step 6: Augment with context
        var summary = await summarizer.SummarizeAsync(query, topChunks);

        return Results.Ok(new
        {
            Id = id,
            Response = summary
        });
    }

    return Results.NotFound();
});

app.MapGet("/assistant/{id:guid}/chat.js", (Guid id, HttpContext context) =>
{
    var scheme = context.Request.Scheme;
    var host = context.Request.Host.Value;
    var baseUrl = $"{scheme}://{host}";
    
    var script = $$"""
(function() {
    'use strict';
    
    const ASSISTANT_ID = '{{id}}';
    const API_BASE_URL = '{{baseUrl}}';
    
    // Create and inject styles
    const styles = `
        .rag-chat-widget {
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 350px;
            height: 500px;
            border: 1px solid #ddd;
            border-radius: 10px;
            background: white;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            display: flex;
            flex-direction: column;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            z-index: 10000;
        }
        
        .rag-chat-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px;
            border-radius: 10px 10px 0 0;
            font-weight: 600;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        .rag-chat-close {
            background: none;
            border: none;
            color: white;
            font-size: 20px;
            cursor: pointer;
            padding: 0;
            width: 24px;
            height: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        .rag-chat-messages {
            flex: 1;
            overflow-y: auto;
            padding: 15px;
            display: flex;
            flex-direction: column;
            gap: 10px;
        }
        
        .rag-chat-message {
            max-width: 80%;
            padding: 10px 14px;
            border-radius: 18px;
            word-wrap: break-word;
            line-height: 1.4;
            font-size: 14px;
        }
        
        .rag-chat-message.user {
            background: #667eea;
            color: white;
            align-self: flex-end;
            border-bottom-right-radius: 4px;
        }
        
        .rag-chat-message.assistant {
            background: #f1f3f4;
            color: #333;
            align-self: flex-start;
            border-bottom-left-radius: 4px;
        }
        
        .rag-chat-message.error {
            background: #fee;
            color: #c33;
            align-self: flex-start;
            border-bottom-left-radius: 4px;
        }
        
        .rag-chat-input-container {
            padding: 15px;
            border-top: 1px solid #eee;
            display: flex;
            gap: 10px;
        }
        
        .rag-chat-input {
            flex: 1;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 20px;
            outline: none;
            font-size: 14px;
            font-family: inherit;
        }
        
        .rag-chat-input:focus {
            border-color: #667eea;
        }
        
        .rag-chat-send {
            background: #667eea;
            color: white;
            border: none;
            border-radius: 20px;
            padding: 10px 20px;
            cursor: pointer;
            font-weight: 600;
            font-size: 14px;
            transition: background 0.2s;
        }
        
        .rag-chat-send:hover:not(:disabled) {
            background: #5568d3;
        }
        
        .rag-chat-send:disabled {
            background: #ccc;
            cursor: not-allowed;
        }
        
        .rag-chat-toggle {
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 60px;
            height: 60px;
            border-radius: 50%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            cursor: pointer;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            font-size: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
        }
        
        .rag-chat-toggle:hover {
            transform: scale(1.05);
        }
        
        .rag-chat-loading {
            display: inline-block;
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #667eea;
            animation: rag-pulse 1.4s infinite ease-in-out both;
        }
        
        .rag-chat-loading:nth-child(1) {
            animation-delay: -0.32s;
        }
        
        .rag-chat-loading:nth-child(2) {
            animation-delay: -0.16s;
        }
        
        @keyframes rag-pulse {
            0%, 80%, 100% {
                opacity: 0.3;
            }
            40% {
                opacity: 1;
            }
        }
        
        .rag-chat-widget.hidden {
            display: none;
        }
    `;
    
    const styleSheet = document.createElement('style');
    styleSheet.textContent = styles;
    document.head.appendChild(styleSheet);
    
    // Create chat widget HTML
    const widgetHTML = `
        <div class="rag-chat-widget hidden" id="ragChatWidget">
            <div class="rag-chat-header">
                <span>Chat Assistant</span>
                <button class="rag-chat-close" id="ragChatClose">×</button>
            </div>
            <div class="rag-chat-messages" id="ragChatMessages">
                <div class="rag-chat-message assistant">
                    Hello! How can I help you today?
                </div>
            </div>
            <div class="rag-chat-input-container">
                <input 
                    type="text" 
                    class="rag-chat-input" 
                    id="ragChatInput" 
                    placeholder="Type your message..."
                    autocomplete="off"
                />
                <button class="rag-chat-send" id="ragChatSend">Send</button>
            </div>
        </div>
        <button class="rag-chat-toggle" id="ragChatToggle">💬</button>
    `;
    
    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initWidget);
    } else {
        initWidget();
    }
    
    function initWidget() {
        const container = document.createElement('div');
        container.innerHTML = widgetHTML;
        document.body.appendChild(container);
        
        const widget = document.getElementById('ragChatWidget');
        const toggle = document.getElementById('ragChatToggle');
        const closeBtn = document.getElementById('ragChatClose');
        const input = document.getElementById('ragChatInput');
        const sendBtn = document.getElementById('ragChatSend');
        const messagesContainer = document.getElementById('ragChatMessages');
        
        let isOpen = false;
        
        function toggleChat() {
            isOpen = !isOpen;
            widget.classList.toggle('hidden', !isOpen);
            toggle.style.display = isOpen ? 'none' : 'flex';
            if (isOpen) {
                input.focus();
            }
        }
        
        toggle.addEventListener('click', toggleChat);
        closeBtn.addEventListener('click', toggleChat);
        
        function addMessage(content, type = 'assistant') {
            const messageDiv = document.createElement('div');
            messageDiv.className = `rag-chat-message ${type}`;
            messageDiv.textContent = content;
            messagesContainer.appendChild(messageDiv);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
        
        function addLoadingIndicator() {
            const loadingDiv = document.createElement('div');
            loadingDiv.className = 'rag-chat-message assistant';
            loadingDiv.id = 'ragChatLoading';
            loadingDiv.innerHTML = '<span class="rag-chat-loading"></span> <span class="rag-chat-loading"></span> <span class="rag-chat-loading"></span>';
            messagesContainer.appendChild(loadingDiv);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
        
        function removeLoadingIndicator() {
            const loading = document.getElementById('ragChatLoading');
            if (loading) {
                loading.remove();
            }
        }
        
        async function sendMessage() {
            const query = input.value.trim();
            if (!query) return;
            
            addMessage(query, 'user');
            input.value = '';
            input.disabled = true;
            sendBtn.disabled = true;
            
            addLoadingIndicator();
            
            try {
                const response = await fetch(`${API_BASE_URL}/assistant/${ASSISTANT_ID}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(query)
                });
                
                removeLoadingIndicator();
                
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                
                const data = await response.json();
                addMessage(data.response || 'No response received', 'assistant');
            } catch (error) {
                removeLoadingIndicator();
                addMessage('Sorry, there was an error processing your request. Please try again.', 'error');
                console.error('Chat error:', error);
            } finally {
                input.disabled = false;
                sendBtn.disabled = false;
                input.focus();
            }
        }
        
        sendBtn.addEventListener('click', sendMessage);
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }
})();
""";
    
    return Results.Content(script, "application/javascript");
})
.WithName("GetChatWidget");

app.Run();

record AssistantRequest(string SourceType, string SourceValue);