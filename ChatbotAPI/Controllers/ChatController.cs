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
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ChatResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Request body is required"
                    });
                }

                var response = await _chatbotService.ProcessMessageAsync(request);
                
                if (!response.IsSuccess)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat endpoint");
                return StatusCode(500, new ChatResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error",
                    Response = "I'm experiencing technical difficulties. Please try again later."
                });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}