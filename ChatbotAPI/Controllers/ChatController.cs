namespace ChatbotAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ChatbotAPI.Models;
    using ChatbotAPI.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatbotService chatbotService, ILogger<ChatController> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
        }

        [HttpPost]
    }
}