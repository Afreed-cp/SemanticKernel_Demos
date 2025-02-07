using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace SemanticKernel_Demos.Plugins
{
#pragma warning disable SKEXP0001
    public class TextEmbeddingPlugin
    {

        private readonly ISemanticTextMemory _memory;
        public TextEmbeddingPlugin(ISemanticTextMemory memory)
        {
            _memory = memory;
        }

        [KernelFunction("search_information")]
        [Description("Gets the movie suggestions based on the genre, type, name etc")]
        public async Task<List<MemoryQueryResult>> SearchAsync(string collection, string query, int limit, float minRelevanceScore)
        {
            var test = await _memory.SearchAsync("embedded_movies", query, limit, 0.1).ToListAsync();
            return test;
        }
    }

    public class SearchResult
    {
        [JsonPropertyName("relevance")]
        public float Relevance { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
