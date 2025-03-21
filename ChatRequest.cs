using System.Text.Json.Serialization;
namespace MyChatBot
{
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public  string Model { get; set; }
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}