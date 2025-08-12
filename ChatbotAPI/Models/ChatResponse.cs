namespace ChatbotAPI.Models
{
    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }
}