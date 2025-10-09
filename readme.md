# Step 1: Install Ollama
Download Ollama

For macOS, visit: https://ollama.com/download/mac

Or install via Homebrew:
```bash
brew install ollama
```

# Step 2: Download models

## Embdedding model
```bash
ollama pull nomic-embed-text
```

## Summarization model
```bash
ollama pull llama3.1:8b
```

# Step 3: Install and run qdrant for Vector storage
Port 6333 is used to open the qdrant manager in browser
Port 6334 is used for grpc client

```bash
docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

# Step 4: Load data
Call API or Console App to load data, create embeddings and save them to vectore store

# Step 5: Search
Call API to Console App with query to search against the stored data





