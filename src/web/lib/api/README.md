# API Client Library

Centralized API client for all backend API calls.

## Overview

This library provides a clean, type-safe interface for interacting with the RAG backend API. All API calls are centralized here, making it easy to:

- Maintain consistent error handling
- Update API endpoints in one place
- Share TypeScript types across components
- Mock API calls for testing
- Add request/response interceptors if needed

## Structure

```
lib/api/
├── index.ts          # Main export file
├── assistants.ts     # Assistants API client
├── datasources.ts    # Data Sources API client
└── README.md         # This file
```

## Usage

### Import the API functions

```typescript
// Import specific functions
import { getAssistants, createAssistant } from "@/lib/api"

// Or import types
import type { Assistant, DataSource } from "@/lib/api"
```

### Assistants API

#### Get all assistants

```typescript
import { getAssistants } from "@/lib/api"

const assistants = await getAssistants()
// Returns: Assistant[]
```

#### Get a single assistant

```typescript
import { getAssistant } from "@/lib/api"

const assistant = await getAssistant("assistant-id")
// Returns: Assistant | null
```

#### Create an assistant

```typescript
import { createAssistant } from "@/lib/api"

const result = await createAssistant({
  name: "My Assistant",
  dataSources: ["datasource-id-1", "datasource-id-2"]
})
// Returns: CreateAssistantResponse
```

#### Query an assistant

```typescript
import { queryAssistant } from "@/lib/api"

const response = await queryAssistant("assistant-id", "What is X?")
// Returns: QueryAssistantResponse
```

#### Delete an assistant

```typescript
import { deleteAssistant } from "@/lib/api"

await deleteAssistant("assistant-id")
// Returns: void
```

#### Update an assistant

```typescript
import { updateAssistant } from "@/lib/api"

await updateAssistant("assistant-id", {
  name: "Updated Name",
  dataSources: ["new-datasource-id"]
})
// Returns: void
```

### Data Sources API

#### Get all data sources

```typescript
import { getDataSources } from "@/lib/api"

const dataSources = await getDataSources()
// Returns: DataSource[]
```

#### Get a single data source

```typescript
import { getDataSource } from "@/lib/api"

const dataSource = await getDataSource("datasource-id")
// Returns: DataSource | null
```

#### Create data sources (upload files)

```typescript
import { createDataSources } from "@/lib/api"

const files: File[] = [file1, file2]
const collectionNames = await createDataSources(files)
// Returns: string[] (collection names)
```

#### Delete a data source

```typescript
import { deleteDataSource } from "@/lib/api"

await deleteDataSource("datasource-id")
// Returns: void
```

#### Get data source type label

```typescript
import { getDataSourceTypeLabel, DataSourceType } from "@/lib/api"

const label = getDataSourceTypeLabel(DataSourceType.File)
// Returns: "File"
```

## Types

### Assistant

```typescript
interface Assistant {
  id: string
  name: string
  dataSources: string[]  // Array of data source IDs
}
```

### CreateAssistantRequest

```typescript
interface CreateAssistantRequest {
  name: string
  dataSources: string[]
}
```

### CreateAssistantResponse

```typescript
interface CreateAssistantResponse {
  id: string
  name: string
}
```

### QueryAssistantResponse

```typescript
interface QueryAssistantResponse {
  response: string
}
```

### DataSource

```typescript
interface DataSource {
  id: string
  dataSourceType: DataSourceType
  dataSourceValue: string
  createdDate: string
  collectionName: string
}
```

### DataSourceType

```typescript
enum DataSourceType {
  Stream = 0,
  File = 1,
  GitHub = 2,
  Url = 3,
  Sitemap = 4,
}
```

## Error Handling

All API functions throw errors on failure. Always wrap calls in try-catch blocks:

```typescript
try {
  const assistants = await getAssistants()
  // Handle success
} catch (error) {
  console.error("Failed to fetch assistants:", error)
  // Handle error
}
```

Error messages include the HTTP status text when available.

## Advanced Usage

### Using the API Client Classes

If you need more control, you can use the client classes directly:

```typescript
import { AssistantsApiClient, DataSourcesApiClient } from "@/lib/api"

// Create a custom instance with a different base URL
const assistantsApi = new AssistantsApiClient("https://api.example.com")
const dataSources = new DataSourcesApiClient("https://api.example.com")

// Use the methods
const assistants = await assistantsApi.getAll()
```

### Singleton Instances

Pre-configured singleton instances are available:

```typescript
import { assistantsApi, dataSourcesApi } from "@/lib/api"

// Use the singleton
const assistants = await assistantsApi.getAll()
```

## Configuration

The API base URL is configured in `lib/config.ts`:

```typescript
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5067"
```

Override it with an environment variable:

```env
NEXT_PUBLIC_API_URL=https://your-api.com
```

## Benefits

### 1. Single Source of Truth

All API calls go through this library, making it easy to:
- Update endpoints
- Add authentication
- Implement retry logic
- Add logging/analytics

### 2. Type Safety

TypeScript interfaces ensure type safety across your application:
- Autocomplete in your IDE
- Compile-time error checking
- Better refactoring support

### 3. Easier Testing

Mock the API functions in tests:

```typescript
jest.mock("@/lib/api", () => ({
  getAssistants: jest.fn(() => Promise.resolve([mockAssistant]))
}))
```

### 4. Consistent Error Handling

All errors follow the same pattern, making error handling predictable.

### 5. Code Reusability

No need to duplicate fetch logic across components.

## Migration from Direct Fetch

**Before:**
```typescript
const response = await fetch("http://localhost:5067/assistants")
const assistants = await response.json()
```

**After:**
```typescript
import { getAssistants } from "@/lib/api"
const assistants = await getAssistants()
```

## Future Enhancements

Potential improvements:

1. **Request Interceptors** - Add authentication headers automatically
2. **Response Caching** - Cache GET requests for better performance
3. **Retry Logic** - Automatically retry failed requests
4. **Request Cancellation** - Cancel in-flight requests
5. **Progress Tracking** - Track upload/download progress
6. **Batch Requests** - Combine multiple requests
7. **WebSocket Support** - Real-time updates
8. **Optimistic Updates** - Update UI before API response

## Contributing

When adding new API endpoints:

1. Add the function to the appropriate client file
2. Export it from `index.ts`
3. Add TypeScript interfaces for request/response
4. Document it in this README
5. Update the integration tests

## Examples

See the following components for real-world usage examples:

- `app/AssistantsClient.tsx` - Fetching assistants
- `app/assistants/chat/ChatClient.tsx` - Querying assistants
- `app/assistants/create/page.tsx` - Creating assistants
- `app/data-sources/DataSourcesClient.tsx` - Managing data sources
- `app/data-sources/create/page.tsx` - Uploading files

