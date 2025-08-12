namespace ChatbotAPI.Models
{
    public class ChatSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public List<ChatMessage> Messages { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Context { get; set; } = new();
    }

    public class ChatMessage
    {
        public string Content { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}