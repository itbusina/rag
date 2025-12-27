docker buildx build --platform linux/amd64 -t rag:amd64 -f src/api/Dockerfile .
docker tag rag:amd64 itbusina/rag:amd64             # Docker Hub