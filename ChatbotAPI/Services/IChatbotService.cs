namespace ChatbotAPI.Services
{
    using ChatbotAPI.Models;

    public interface IChatbotService
    {
        Task<ChatResponse> ProcessMessageAsync(ChatRequest request);
        Task<string> GenerateResponseAsync(string userMessage, List<ChatMessage> conversationHistory);
        ChatSession GetOrCreateSession(string? sessionId);
        void CleanupExpiredSessions();
    }
}