using System.Net;
using System.Runtime.CompilerServices;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIChatService _aiService;

        public AIController(IAIChatService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequest request, CancellationToken ct)
        {
            if (request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest(new { error = "messages is required" });
            }

            try
            {
                var resp = await _aiService.ChatAsync(request, ct);
                return Ok(new
                {
                    content = resp.Content,
                    usage = new { resp.PromptTokens, resp.CompletionTokens, resp.TotalTokens }
                });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                return StatusCode(401, "AI provider unauthorized: please set OPENAI_API_KEY or User Secrets.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("chat/stream")]
        public async Task ChatStream([FromBody] AiChatRequest request, CancellationToken ct)
        {
            Response.ContentType = "text/event-stream";
            await foreach (var piece in _aiService.ChatStreamAsync(request, ct))
            {
                await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { content = piece })}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
    }
}


