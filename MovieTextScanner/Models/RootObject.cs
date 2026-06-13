using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieTextScanner
{
    public class RootObject
    {
        [JsonPropertyName("figures")]
        public List<Figure> Figures { get; set; }
        [JsonPropertyName("paragraphs")]
        public List<object> Paragraphs { get; set; }
        [JsonPropertyName("tables")]
        public List<object> Tables { get; set; }
        [JsonPropertyName("words")]
        public List<Word> Words { get; set; }
    }
}
