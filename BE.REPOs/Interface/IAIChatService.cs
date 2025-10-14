using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public record AiChatMessage(string Role, string Content);

    public class AiChatRequest
    {
        public List<AiChatMessage> Messages { get; set; } = new List<AiChatMessage>();
        public double? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public bool Stream { get; set; }
    }

    public class AiChatResponse
    {
        public string Content { get; set; } = string.Empty;
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? TotalTokens { get; set; }
    }

    public interface IAIChatService
    {
        Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> ChatStreamAsync(AiChatRequest request, CancellationToken cancellationToken = default);
    }
}


