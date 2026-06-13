using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieTextScanner
{
    public class Figure
    {
        [JsonPropertyName("box")]
        public List<int> Box { get; set; }
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "";
        [JsonPropertyName("order")]
        public int Order { get; set; }
        [JsonPropertyName("paragraphs")]
        public List<Paragraph> Paragraphs { get; set; }
    }
}
