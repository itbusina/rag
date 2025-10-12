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





