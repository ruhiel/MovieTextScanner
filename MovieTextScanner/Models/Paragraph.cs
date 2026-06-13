using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieTextScanner
{
    public class Paragraph
    {
        [JsonPropertyName("box")]
        public List<int> Box { get; set; }
        [JsonPropertyName("contents")]
        public string Contents { get; set; }
        [JsonPropertyName("direction")]
        public string Direction { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
