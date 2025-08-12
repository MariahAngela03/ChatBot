namespace ChatbotAPI.Models
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }
}