using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BE.REPOs.Interface;

namespace BE.REPOs.Service
{
    public class OpenRouterChatService : IAIChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAIOptions _options;

        public OpenRouterChatService(IHttpClientFactory httpClientFactory, OpenAIOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public async Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("OpenRouter");

            var payload = new ChatCompletionsRequest
            {
                Model = _options.Model,
                Messages = request.Messages.Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content }).ToList(),
                Temperature = request.Temperature ?? _options.DefaultTemperature,
                MaxTokens = request.MaxTokens ?? _options.DefaultMaxTokens,
                Stream = false
            };

            using var response = await client.PostAsJsonAsync("chat/completions", payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                // Bubble up precise status for the controller to map (e.g., 401)
                throw new HttpRequestException(
                    $"OpenAI-compatible API error: {(int)response.StatusCode} {response.ReasonPhrase}",
                    null,
                    response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<ChatCompletionsResponse>(cancellationToken: cancellationToken)
                       ?? throw new InvalidOperationException("Cannot parse completion response");

            var content = data.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
            var usage = data.Usage;
            return new AiChatResponse
            {
                Content = content,
                PromptTokens = usage?.PromptTokens,
                CompletionTokens = usage?.CompletionTokens,
                TotalTokens = usage?.TotalTokens
            };
        }

        public async IAsyncEnumerable<string> ChatStreamAsync(AiChatRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("OpenRouter");

            var payload = new ChatCompletionsRequest
            {
                Model = _options.Model,
                Messages = request.Messages.Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content }).ToList(),
                Temperature = request.Temperature ?? _options.DefaultTemperature,
                MaxTokens = request.MaxTokens ?? _options.DefaultMaxTokens,
                Stream = true
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(payload)
            };
            using var httpResponse = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"OpenAI-compatible API error: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}",
                    null,
                    httpResponse.StatusCode);
            }

            using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("data:")) continue;
                var data = line.Substring("data:".Length).Trim();
                if (data == "[DONE]") yield break;

                var chunk = JsonSerializer.Deserialize<ChatCompletionsStreamChunk>(data);
                var delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(delta))
                {
                    yield return delta;
                }
            }
        }

        private class ChatCompletionsRequest
        {
            [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
            [JsonPropertyName("messages")] public List<ChatMessageDto> Messages { get; set; } = new();
            [JsonPropertyName("temperature")] public double Temperature { get; set; }
            [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
            [JsonPropertyName("stream")] public bool Stream { get; set; }
        }

        private class ChatMessageDto
        {
            [JsonPropertyName("role")] public string Role { get; set; } = "user";
            [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
        }

        private class ChatCompletionsResponse
        {
            [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
            [JsonPropertyName("usage")] public Usage? Usage { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("message")] public Message? Message { get; set; }
        }

        private class Message
        {
            [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
            [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
        }

        private class Usage
        {
            [JsonPropertyName("prompt_tokens")] public int? PromptTokens { get; set; }
            [JsonPropertyName("completion_tokens")] public int? CompletionTokens { get; set; }
            [JsonPropertyName("total_tokens")] public int? TotalTokens { get; set; }
        }

        private class ChatCompletionsStreamChunk
        {
            [JsonPropertyName("choices")] public List<StreamChoice>? Choices { get; set; }
        }

        private class StreamChoice
        {
            [JsonPropertyName("delta")] public Delta? Delta { get; set; }
        }

        private class Delta
        {
            [JsonPropertyName("content")] public string? Content { get; set; }
        }
    }
}


