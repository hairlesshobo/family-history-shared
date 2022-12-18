using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared
{
    public static class Static
    {
        public static readonly JsonSerializerOptions DefaultJso = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }
}