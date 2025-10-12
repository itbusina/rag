#!/bin/bash
set -e

# Start Ollama in the background
echo "Starting Ollama server..."
/bin/ollama serve &

# Store the PID of the Ollama server
OLLAMA_PID=$!

# Wait for Ollama to be ready
echo "Waiting for Ollama server to be ready..."
until ollama list > /dev/null 2>&1; do
    echo "Waiting for Ollama API..."
    sleep 2
done

echo "Ollama server is ready!"

# Pull required models
echo "Pulling embedding model: $EMBEDDING_MODEL..."
ollama pull "$EMBEDDING_MODEL"

echo "Pulling summarizing model: $SUMMARIZING_MODEL..."
ollama pull "$SUMMARIZING_MODEL"

echo "All models downloaded successfully!"
echo "Ollama is ready to serve requests."

# Keep the container running by waiting for the Ollama process
wait $OLLAMA_PID

