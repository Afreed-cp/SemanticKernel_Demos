# Semantic Kernel

This project is a **Semantic Kernel Demo** built using **Microsoft Semantic Kernel**, **Ollama**, and **MongoDB Atlas**. It integrates **semantic search**, **LLM-based chat completion**, and **memory storage** to provide personalized movie recommendations based on embedded movie descriptions.

## Features
- **Semantic search** using OpenAI embeddings via Ollama
- **Chat-based recommendation system** using Llama3-Groq
- **Memory storage for movie embeddings** using VolatileMemoryStore
- **Integration with MongoDB Atlas** for retrieving and storing movie data
- **Custom plugins** for embedding text and searching information

## Technologies Used
- **Microsoft Semantic Kernel**
- **Ollama** (Ollama API client for embeddings)
- **MongoDB Atlas**
- **.NET 9**
- **C#**

## Setup and Installation

### Prerequisites
- Install **.NET 9**
- Setup a **MongoDB Atlas** account and get the connection string
- Get **Ollama API** endpoint details

### Configuration
1. Clone the repository:
   ```sh
   git clone https://github.com/your-username/your-repo.git
   cd your-repo
   ```
2. Open the project in **Visual Studio** or your preferred IDE.
3. Update the following environment variables or constants in `Program.cs`:
   - **OllamaEndpoint**: Your Ollama API endpoint
   - **MongoDBAtlasConnectionString**: Your MongoDB Atlas connection string
   - **CollectionName**: MongoDB collection storing movie data
4. Restore dependencies:
   ```sh
   dotnet restore
   ```
5. Build the project:
   ```sh
   dotnet build
   ```

## Running the Application
To start the application, run:
```sh
   dotnet run
```

### Usage
1. The system will **retrieve movie data** from MongoDB Atlas.
2. It will **embed movie descriptions** using the Ollama embedding model.
3. Users can enter queries in the console to **receive movie recommendations**.

## Plugins
### `TextEmbeddingPlugin`
- Provides **semantic search** for movies.
- Searches by **genre, name, and type**.
- Uses Semantic Kernel's `ISemanticTextMemory`.

### Example Query
```sh
User > Recommend me a sci-fi movie with time travel.
Assistant > You might like "Interstellar". Plot: ...
```
