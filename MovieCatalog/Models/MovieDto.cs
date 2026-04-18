using System;
using System.Text.Json.Serialization;

namespace MovieCatalog.Models
{
	internal class MovieDto
	{
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}

