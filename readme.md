# RAG Application

This is a Retrieval-Augmented Generation (RAG) application built with .NET 9 and Next.js.

## Quick Start with Docker Compose (Recommended)

The easiest way to run the entire application is using Docker Compose:

```bash
docker-compose up
```

This will:
- Start the Ollama service and automatically download required models (`nomic-embed-text` and `llama3.1:8b`)
- Start the Qdrant vector database
- Build and run the API with the embedded Next.js frontend

The application will be available at: http://localhost:8080

**Note:** The first startup will take longer as Ollama downloads the required models (~4-5GB).

### macOS Users - Important Performance Note

If you are using macOS, it is **recommended to run Ollama locally** and point the RAG application to this local instance instead of running Ollama in Docker. This is due to performance constraints of running Ollama in Docker on Mac.

To connect the RAG application running in Docker to your local Ollama instance:

1. Install and run Ollama locally (see Manual Setup section below)
2. Update the Ollama URL in the API configuration to: `http://host.docker.internal:11434`

This setup provides significantly better performance for embeddings and model inference on macOS.

### Stopping the services
```bash
docker-compose down
```

### View logs
```bash
docker-compose logs -f
```

---

## Manual Setup (Alternative)

If you prefer to run the services manually:

### Step 1: Install Ollama
Download Ollama

For macOS, visit: https://ollama.com/download/mac

Or install via Homebrew:
```bash
brew install ollama
```

### Step 2: Download models

#### Embedding model
```bash
ollama pull nomic-embed-text
```

#### Summarization model
```bash
ollama pull llama3.1:8b
```

### Step 3: Install and run qdrant for Vector storage
Port 6333 is used to open the qdrant manager in browser
Port 6334 is used for grpc client

```bash
docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

### Step 4: Add data source(s)
Run application ```dotnet run``` in /api project. Open http://localhost:5067 to see the dashboard. Open Data sources and add some documents. 

### Step 5: Create Assistant
Open 'Create Assistant' page, add data sources and create the assistant.

### Step 6: Chat with assistant
Open the chat for the assistant and start searching across the data sources.

---

## Cloud Setup
1. Setup account in https://cloud.qdrant.io/
2. Create cluster, copy qdrant URL and API Key.
3. Setup OpenAI account
4. Create OpenAI service account https://platform.openai.com/api-keys
5. Deploy the api application to cloud (Azure) with docker
6. Configure environment variables to point OpenAI API and Qdrant, set SERVICE_PROVIDER to openai
7. Set DATA_STORAGE_CONNECTION_STRING to /home/rag.db. /home is a folder which is preserved and not removed after service restart.
7. Run the application.


## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

