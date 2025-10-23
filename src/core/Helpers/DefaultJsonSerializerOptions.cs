using System.Text.Json;

namespace core.Helpers
{
    public static class DefaultJsonSerializerOptions
    {
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
        {
            WriteIndented = true, // Enables pretty formatting
            PropertyNameCaseInsensitive = true
        };
    }
}