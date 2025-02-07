using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using OllamaSharp;
using SemanticKernel_Demos.Models;
using Kernel = Microsoft.SemanticKernel.Kernel;
using MongoDB.Driver;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using SemanticKernel_Demos.Plugins;

namespace SemanticKernel_Demos;


// Features are still experimental
#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050, SKEXP0070 
public static partial class Program
{
    static string TextEmbeddingModelName = "chroma/all-minilm-l6-v2-f32";
    static string OllamaEndpoint = "";
    static string CollectionName = "embedded_movies";
    static string MongoDBAtlasConnectionString = "";
    static MemoryBuilder memoryBuilder;

    public static async Task Main(string[] args)
    {
        memoryBuilder = new MemoryBuilder();

        memoryBuilder.WithTextEmbeddingGeneration((loggerFactory, httpClient) =>
        {
            var client = new OllamaApiClient(OllamaEndpoint, TextEmbeddingModelName);
            return client.AsTextEmbeddingGenerationService();
        });
        
        var builder = Kernel.CreateBuilder();
        
        // Initialize memory services
        var memoryStore = new VolatileMemoryStore();
        builder.Services.AddSingleton<IMemoryStore>(memoryStore);
        builder.Services.AddSingleton<ISemanticTextMemory>(sp => 
        {
            var store = sp.GetRequiredService<IMemoryStore>();
            var embedder = new OllamaApiClient(OllamaEndpoint, TextEmbeddingModelName)
                .AsTextEmbeddingGenerationService();
            return new SemanticTextMemory(store, embedder);
        });

        // Initialize the kernel with chat completion model
        builder.Services.AddOllamaChatCompletion("llama3-groq-tool-use:latest", new Uri(OllamaEndpoint));
        //Plugins
        builder.Plugins.AddFromType<TextEmbeddingPlugin>();
        builder.Plugins.AddFromType<LightsPlugin>();

        Kernel kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var history = new ChatHistory();

        memoryBuilder.WithMemoryStore(memoryStore);
        var memory = memoryBuilder.Build();

        // Remove memory entries and save new ones
        await SaveInformationAndClearCollection(memory);

        string? userInput;
        do
        {
            Console.Write("User > ");
            userInput = Console.ReadLine();
            if (userInput == null) break;

            history.AddUserMessage(userInput);
            var result = await chatCompletionService.GetChatMessageContentsAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            Console.WriteLine("Assistant > " + result.FirstOrDefault()?.Content);
        } while (true);
    }

    private static async Task SaveInformationAndClearCollection(ISemanticTextMemory memory)
    {
        await ClearCollection(memory);
        var mongoClient = new MongoClient(MongoDBAtlasConnectionString);
        var movieDB = mongoClient.GetDatabase("sample_mflix");
        var movieCollection = movieDB.GetCollection<Movie>("embedded_movies");
        var movieDocuments = movieCollection.Find(m => true).Limit(50).ToList();

        foreach (var movie in movieDocuments)
        {
            try
            {
                Console.WriteLine($"\nSaving {movie.Id}...");
                await memory.SaveInformationAsync(
                    collection: CollectionName,
                    text: movie.Plot,
                    id: movie.Id,
                    description: $"Movie {movie.Title} description for {movie.Id} with plot {movie.Plot}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving {movie.Id}: {ex.Message}");
            }
        }
    }
    private static async Task ClearCollection(ISemanticTextMemory memory)
    {
        try
        {
            Console.WriteLine($"Attempting to clear collection: {CollectionName}");

            // Get all entries
            var allEntries = await memory.SearchAsync(
                collection: CollectionName,
                query: "*",
                limit: 1000  // Adjust if you have more entries
            ).ToListAsync();

            // Remove each entry by its ID
            foreach (var entry in allEntries)
            {
                await memory.RemoveAsync(CollectionName, entry.Metadata.Id);
                Console.WriteLine($"Removed entry with ID: {entry.Metadata.Id}");
            }

            Console.WriteLine($"Successfully cleared collection: {CollectionName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: Error clearing collection: {ex.Message}");
        }
    }
}
