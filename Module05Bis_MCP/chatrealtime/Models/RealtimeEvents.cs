using System.Text.Json.Serialization;

namespace chatrealtime.Models;

// Base event
public abstract class RealtimeEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }
}

// Session configuration
public class SessionUpdateEvent : RealtimeEvent
{
    public SessionUpdateEvent()
    {
        Type = "session.update";
    }

    [JsonPropertyName("session")]
    public SessionConfig Session { get; set; } = new();
}

public class SessionConfig
{
    [JsonPropertyName("modalities")]
    public string[] Modalities { get; set; } = new[] { "text", "audio" };

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    [JsonPropertyName("input_audio_format")]
    public string InputAudioFormat { get; set; } = "pcm16";

    [JsonPropertyName("output_audio_format")]
    public string OutputAudioFormat { get; set; } = "pcm16";

    [JsonPropertyName("input_audio_transcription")]
    public TranscriptionConfig? InputAudioTranscription { get; set; }

    [JsonPropertyName("turn_detection")]
    public TurnDetectionConfig? TurnDetection { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("max_response_output_tokens")]
    public int? MaxResponseOutputTokens { get; set; }

    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public string? ToolChoice { get; set; } = "auto";
}

public class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public object Parameters { get; set; } = new();
}

public class TranscriptionConfig
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "whisper-1";
}

public class TurnDetectionConfig
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "server_vad";

    [JsonPropertyName("threshold")]
    public double? Threshold { get; set; }

    [JsonPropertyName("prefix_padding_ms")]
    public int? PrefixPaddingMs { get; set; }

    [JsonPropertyName("silence_duration_ms")]
    public int? SilenceDurationMs { get; set; }
}

// Input audio events
public class InputAudioBufferAppendEvent : RealtimeEvent
{
    public InputAudioBufferAppendEvent()
    {
        Type = "input_audio_buffer.append";
    }

    [JsonPropertyName("audio")]
    public string Audio { get; set; } = string.Empty;
}

public class InputAudioBufferCommitEvent : RealtimeEvent
{
    public InputAudioBufferCommitEvent()
    {
        Type = "input_audio_buffer.commit";
    }
}

public class InputAudioBufferClearEvent : RealtimeEvent
{
    public InputAudioBufferClearEvent()
    {
        Type = "input_audio_buffer.clear";
    }
}

// Response events
public class ResponseCreateEvent : RealtimeEvent
{
    public ResponseCreateEvent()
    {
        Type = "response.create";
    }

    [JsonPropertyName("response")]
    public ResponseConfig? Response { get; set; }
}

public class ResponseConfig
{
    [JsonPropertyName("modalities")]
    public string[]? Modalities { get; set; }

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }
}

public class ResponseCancelEvent : RealtimeEvent
{
    public ResponseCancelEvent()
    {
        Type = "response.cancel";
    }
}

// Conversation item events
public class ConversationItemCreateEvent : RealtimeEvent
{
    public ConversationItemCreateEvent()
    {
        Type = "conversation.item.create";
    }

    [JsonPropertyName("item")]
    public object Item { get; set; } = new();
}

public class FunctionCallOutputItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function_call_output";

    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }

    [JsonPropertyName("output")]
    public string? Output { get; set; }
}

public class UserMessageItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "message";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    [JsonPropertyName("content")]
    public List<MessageContent> Content { get; set; } = new();
}

public class MessageContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

// Server events (received from OpenAI)
public class SessionCreatedEvent : RealtimeEvent
{
    [JsonPropertyName("session")]
    public SessionConfig? Session { get; set; }
}

public class SessionUpdatedEvent : RealtimeEvent
{
    [JsonPropertyName("session")]
    public SessionConfig? Session { get; set; }
}

public class ConversationItemCreatedEvent : RealtimeEvent
{
    [JsonPropertyName("previous_item_id")]
    public string? PreviousItemId { get; set; }

    [JsonPropertyName("item")]
    public ConversationItem? Item { get; set; }
}

public class ConversationItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public ContentItem[]? Content { get; set; }
}

public class ContentItem
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class InputAudioBufferCommittedEvent : RealtimeEvent
{
    [JsonPropertyName("previous_item_id")]
    public string? PreviousItemId { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }
}

public class InputAudioBufferSpeechStartedEvent : RealtimeEvent
{
    [JsonPropertyName("audio_start_ms")]
    public int AudioStartMs { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }
}

public class InputAudioBufferSpeechStoppedEvent : RealtimeEvent
{
    [JsonPropertyName("audio_end_ms")]
    public int AudioEndMs { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }
}

public class ResponseAudioTranscriptDeltaEvent : RealtimeEvent
{
    [JsonPropertyName("response_id")]
    public string? ResponseId { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }

    [JsonPropertyName("content_index")]
    public int ContentIndex { get; set; }

    [JsonPropertyName("delta")]
    public string? Delta { get; set; }
}

public class ResponseAudioTranscriptDoneEvent : RealtimeEvent
{
    [JsonPropertyName("response_id")]
    public string? ResponseId { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }

    [JsonPropertyName("content_index")]
    public int ContentIndex { get; set; }

    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }
}

public class ResponseAudioDeltaEvent : RealtimeEvent
{
    [JsonPropertyName("response_id")]
    public string? ResponseId { get; set; }

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("output_index")]
    public int OutputIndex { get; set; }

    [JsonPropertyName("content_index")]
    public int ContentIndex { get; set; }

    [JsonPropertyName("delta")]
    public string? Delta { get; set; }
}

public class ResponseDoneEvent : RealtimeEvent
{
    [JsonPropertyName("response")]
    public ResponseData? Response { get; set; }
}

public class ResponseData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("output")]
    public OutputItem[]? Output { get; set; }

    [JsonPropertyName("usage")]
    public UsageData? Usage { get; set; }
}

public class UsageData
{
    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; set; }

    [JsonPropertyName("input_tokens")]
    public int? InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int? OutputTokens { get; set; }

    [JsonPropertyName("input_token_details")]
    public InputTokenDetails? InputTokenDetails { get; set; }

    [JsonPropertyName("output_token_details")]
    public OutputTokenDetails? OutputTokenDetails { get; set; }
}

public class InputTokenDetails
{
    [JsonPropertyName("cached_tokens")]
    public int? CachedTokens { get; set; }

    [JsonPropertyName("text_tokens")]
    public int? TextTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    public int? AudioTokens { get; set; }
}

public class OutputTokenDetails
{
    [JsonPropertyName("text_tokens")]
    public int? TextTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    public int? AudioTokens { get; set; }
}

public class OutputItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public ContentItem[]? Content { get; set; }
}

public class ErrorEvent : RealtimeEvent
{
    [JsonPropertyName("error")]
    public ErrorDetails? Error { get; set; }
}

public class ErrorDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }
}
