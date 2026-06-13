using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieTextScanner
{
    public class Word
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("det_score")]
        public double DetScore { get; set; }
        [JsonPropertyName("direction")]
        public string Direction { get; set; }
        [JsonPropertyName("points")]
        public List<List<int>> Points { get; set; }
        [JsonPropertyName("rec_score")]
        public double RecScore { get; set; }
    }
}
