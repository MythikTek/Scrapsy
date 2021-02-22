using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace scrapsy.Stores.Data
{
    public class BestBuyConfig
    {
        [JsonPropertyName("product_data")] public List<BestBuyProductData> ProductData { get; set; } = new();

        [JsonPropertyName("timeout")] public int Timeout { get; set; }

        [JsonPropertyName("delay")] public int Delay { get; set; }
    }

    public class BestBuyProductData
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("sku")] public string Sku { get; set; }

        [JsonPropertyName("model_number")] public string ModelNumber { get; set; }

        [JsonPropertyName("minimum_price")] public int MinimumPrice { get; set; }

        [JsonPropertyName("maximum_price")] public int MaximumPrice { get; set; }
    }
}