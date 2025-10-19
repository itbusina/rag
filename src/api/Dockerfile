# Stage 1: Build Next.js frontend
FROM node:24-alpine AS frontend-builder

# Define build argument for API URL
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}

WORKDIR /app/web

# Copy package files
COPY src/web/package*.json ./

# Install dependencies
RUN npm ci

# Copy web project files
COPY src/web/ ./

# Build Next.js app (outputs to 'out' folder)
RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS api-builder

WORKDIR /app

# Copy project files for restore
COPY src/core/core.csproj ./core/
COPY src/api/api.csproj ./api/

# Restore dependencies
RUN dotnet restore ./api/api.csproj

# Copy source code
COPY src/core/ ./core/
COPY src/api/ ./api/

# Clean wwwroot folder
RUN rm -rf ./api/wwwroot/*

# Copy Next.js build output from frontend-builder stage to wwwroot
COPY --from=frontend-builder /app/web/out/ ./api/wwwroot/
COPY /src/api/demo ./api/wwwroot/

# Build the API project
RUN dotnet publish ./api/api.csproj -c Release -o /app/publish

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

WORKDIR /app

# Copy published API from builder stage
COPY --from=api-builder /app/publish .

# Expose port (adjust if your app uses a different port)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "api.dll"]

