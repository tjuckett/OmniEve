using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OmniEveMarket
{
    public class JsonTypesPage
    {
        [JsonProperty(PropertyName = "totalCount")]
        public int TotalCount { get; set; }
        [JsonProperty(PropertyName = "pageCount")]
        public int PageCount { get; set; }
        [JsonProperty(PropertyName = "items")]
        public List<JsonTypesItem> Items { get; set; }
        [JsonProperty(PropertyName = "next")]
        public JsonTypesLink Next { get; set; }
        [JsonProperty(PropertyName = "previous")]
        public JsonTypesLink Previous { get; set; }
    }

    public class JsonTypesGroup
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }

    public class JsonTypesLink
    {
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
    }

    public class JsonTypesType
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "icon")]
        public JsonTypesLink Icon { get; set; }
    }

    public class JsonTypesItem
    {
        [JsonProperty(PropertyName = "marketGroup")]
        public JsonTypesGroup Group { get; set; }
        [JsonProperty(PropertyName = "type")]
        public JsonTypesType Type { get; set; }
    }

    public class JsonHistoryPage
    {
        [JsonProperty(PropertyName = "totalCount")]
        public int TotalCount { get; set; }
        [JsonProperty(PropertyName = "pageCount")]
        public int PageCount { get; set; }
        [JsonProperty(PropertyName = "items")]
        public List<JsonHistoryItem> Items { get; set; }
        [JsonProperty(PropertyName = "next")]
        public JsonTypesLink Next { get; set; }
        [JsonProperty(PropertyName = "previous")]
        public JsonTypesLink Previous { get; set; }
    }

    public class JsonHistoryItem
    {
        [JsonProperty(PropertyName = "volume")]
        public long Volume { get; set; }
        [JsonProperty(PropertyName = "orderCount")]
        public int OrderCount { get; set; }
        [JsonProperty(PropertyName = "lowPrice")]
        public decimal LowPrice { get; set; }
        [JsonProperty(PropertyName = "highPrice")]
        public decimal HighPrice { get; set; }
        [JsonProperty(PropertyName = "avgPrice")]
        public decimal AvgPrice { get; set; }
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
    }
}
