namespace core.ChatClients.Models
{
    public class AIClientMessage
    {
        public required string Role { get; set; }
        public required string Content { get; set; }
    }
}