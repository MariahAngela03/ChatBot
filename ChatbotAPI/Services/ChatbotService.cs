namespace ChatbotAPI.Services
{
    using ChatbotAPI.Models;
    using System.Collections.Concurrent;
    using System.Text.Json;

    public class ChatbotService : IChatbotService
    {
        private static readonly ConcurrentDictionary<string, ChatSession> _sessions = new();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(IHttpClientFactory httpClientFactory, ILogger<ChatbotService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            // Start cleanup timer for expired sessions
            var timer = new Timer((_) => CleanupExpiredSessions(), null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }

        public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return new ChatResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Message cannot be empty"
                    };
                }

                // Get or create session
                var session = GetOrCreateSession(request.SessionId);

                // Add user message to session
                session.Messages.Add(new ChatMessage
                {
                    Content = request.Message,
                    IsFromUser = true
                });

                // Generate bot response
                var botResponse = await GenerateResponseAsync(request.Message, session.Messages);

                // Add bot response to session
                session.Messages.Add(new ChatMessage
                {
                    Content = botResponse,
                    IsFromUser = false
                });

                // Update session activity
                session.LastActivity = DateTime.UtcNow;

                _logger.LogInformation($"Processed message for session {session.SessionId}");

                return new ChatResponse
                {
                    Response = botResponse,
                    SessionId = session.SessionId,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return new ChatResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while processing your message",
                    Response = "I'm sorry, I encountered an error. Please try again."
                };
            }
        }

        public async Task<string> GenerateResponseAsync(string userMessage, List<ChatMessage> conversationHistory)
        {
            // This is where you would integrate with your preferred AI service
            // Examples: OpenAI GPT, Azure OpenAI, Google Gemini, local models, etc.
            
            try
            {
                // Example: Call to external AI service
                // var response = await CallExternalAIService(userMessage, conversationHistory);
                // return response;

                // For now, return rule-based responses
                return GenerateRuleBasedResponse(userMessage, conversationHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response");
                return "I'm having trouble processing your request right now. Please try again.";
            }
        }

        private string GenerateRuleBasedResponse(string userMessage, List<ChatMessage> conversationHistory)
        {
            var message = userMessage.ToLower().Trim();

            // Greeting responses
            if (IsGreeting(message))
            {
                var greetings = new[]
                {
                    "Hello! How can I help you today?",
                    "Hi there! What can I assist you with?",
                    "Greetings! I'm here to help. What's on your mind?",
                    "Hello! Nice to meet you. What would you like to talk about?"
                };
                return greetings[Random.Shared.Next(greetings.Length)];
            }

            // Help requests
            if (message.Contains("help") || message.Contains("what can you do"))
            {
                return "I'm an AI assistant that can help you with:\n" +
                       "• Answering questions\n" +
                       "• Having conversations\n" +
                       "• Providing information on various topics\n" +
                       "• Assisting with problem-solving\n" +
                       "What specific area would you like help with?";
            }

            // Joke requests
            if (message.Contains("joke") || message.Contains("funny"))
            {
                var jokes = new[]
                {
                    "Why don't programmers like nature? It has too many bugs!",
                    "Why did the AI go to therapy? It had deep learning issues!",
                    "What do you call a chatbot that can sing? A soprano-bot!",
                    "Why don't robots ever panic? They have great self-control systems!"
                };
                return jokes[Random.Shared.Next(jokes.Length)];
            }

            // Time/date requests
            if (message.Contains("time") || message.Contains("date"))
            {
                return $"The current time is {DateTime.Now:HH:mm} and today's date is {DateTime.Now:MMMM dd, yyyy}.";
            }

            // Weather (placeholder - you'd integrate with weather API)
            if (message.Contains("weather"))
            {
                return "I don't have access to real-time weather data yet, but you can check your local weather service for current conditions. Is there anything else I can help you with?";
            }

            // Default responses
            var defaultResponses = new[]
            {
                "That's an interesting topic! Could you tell me more about what you'd like to know?",
                "I'd be happy to help with that. Can you provide more details?",
                "That's a great question. Let me think about that for you.",
                "I understand you're asking about that. What specific aspect interests you most?",
                "Thanks for sharing that with me. How can I best assist you with this topic?"
            };

            return defaultResponses[Random.Shared.Next(defaultResponses.Length)];
        }

        private static bool IsGreeting(string message)
        {
            var greetingWords = new[] { "hello", "hi", "hey", "greetings", "good morning", "good afternoon", "good evening" };
            return greetingWords.Any(greeting => message.Contains(greeting));
        }

        public ChatSession GetOrCreateSession(string? sessionId)
        {
            if (string.IsNullOrEmpty(sessionId) || !_sessions.TryGetValue(sessionId, out var session))
            {
                session = new ChatSession();
                _sessions.TryAdd(session.SessionId, session);
            }

            return session;
        }

        public void CleanupExpiredSessions()
        {
            var expiredSessions = _sessions
                .Where(kvp => DateTime.UtcNow - kvp.Value.LastActivity > TimeSpan.FromHours(24))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                _sessions.TryRemove(sessionId, out _);
            }

            if (expiredSessions.Any())
            {
                _logger.LogInformation($"Cleaned up {expiredSessions.Count} expired sessions");
            }
        }

        // Example method for integrating with external AI services
        private async Task<string> CallExternalAIService(string userMessage, List<ChatMessage> conversationHistory)
        {
            // Example integration with OpenAI (you'll need to add OpenAI NuGet package and API key)
            /*
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_OPENAI_API_KEY");

            var messages = conversationHistory.TakeLast(10).Select(m => new
            {
                role = m.IsFromUser ? "user" : "assistant",
                content = m.Content
            }).ToList();

            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = messages,
                max_tokens = 150,
                temperature = 0.7
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
            
            return result.choices[0].message.content;
            */

            await Task.Delay(100); // Simulate API call delay
            return "This would be a response from an external AI service.";
        }
    }
}