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
echo "Pulling nomic-embed-text model..."
ollama pull nomic-embed-text

echo "Pulling llama3.1:8b model..."
ollama pull llama3.1:8b

echo "All models downloaded successfully!"
echo "Ollama is ready to serve requests."

# Keep the container running by waiting for the Ollama process
wait $OLLAMA_PID

