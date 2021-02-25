using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace scrapsy.Stores.Data
{
    public class BestBuyConfig
    {
        [JsonPropertyName("links")] public List<string> Links { get; set; } = new List<string>();

        [JsonPropertyName("timeout")] public int Timeout { get; set; } = 30;

        [JsonPropertyName("delay")] public int Delay { get; set; } = 4000;
    }
}