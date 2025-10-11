# API Integration Summary

This document describes how the Next.js web application integrates with the backend API.

## Overview

The web application is now fully integrated with the C# backend API running on `http://localhost:5067`. All pages fetch real data from the API and submit changes back to it.

## Integrated Pages

### 1. Home Page (Assistants List)
**File**: `app/page.tsx` → `app/AssistantsClient.tsx`

**Features**:
- Fetches all assistants from `GET /assistants`
- Displays assistant name and connected data sources count
- Links to chat and edit pages
- Loading and error states

**API Response Model**:
```typescript
{
  id: string
  name: string
  dataSources: string[]  // Array of data source IDs
}
```

### 2. Create Assistant Page
**File**: `app/assistants/create/page.tsx`

**Features**:
- Fetches available data sources from `GET /datasources`
- Allows selecting multiple data sources
- Submits to `POST /assistants` with:
  ```json
  {
    "name": "Assistant Name",
    "dataSources": ["guid1", "guid2"]
  }
  ```
- Redirects to home page on success

### 3. Chat Page
**File**: `app/assistants/chat/page.tsx` → `ChatClient.tsx`
**URL**: `/assistants/chat?id={assistantId}`

**Features**:
- Fetches assistant details to display name
- Sends user messages to `POST /assistants/{id}`
- Request body: JSON string of the user's question
- Response: `{ "response": "AI answer" }`
- Real-time chat interface with message history

### 4. Data Sources List
**File**: `app/data-sources/page.tsx` → `DataSourcesClient.tsx`

**Features**:
- Fetches all data sources from `GET /datasources`
- Displays data source type, value, and creation date
- Delete functionality via `DELETE /datasources/{id}`
- Auto-refreshes list after deletion

**API Response Model**:
```typescript
{
  id: string
  dataSourceType: DataSourceType  // 0=Stream, 1=File, 2=GitHub, 3=Url, 4=Sitemap
  dataSourceValue: string
  createdDate: string
  collectionName: string
}
```

### 5. Create Data Source Page
**File**: `app/data-sources/create/page.tsx`

**Features**:
- File upload interface
- Submits files via `POST /datasources` as multipart/form-data
- Backend processes each file and creates data source entries
- Shows upload progress and success/error messages
- Redirects to data sources list on success

## Data Models

### DataSourceType Enum
```csharp
public enum DataSourceType
{
    Stream = 0,
    File = 1,
    GitHub = 2,
    Url = 3,
    Sitemap = 4,
}
```

Mapped to display labels:
- 0 → "Stream"
- 1 → "File"
- 2 → "GitHub"
- 3 → "URL"
- 4 → "Sitemap"

### Assistant Model (Backend)
```csharp
public class Assistant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public ICollection<DataSource> DataSources { get; set; } = [];
}
```

### DataSource Model (Backend)
```csharp
public class DataSource
{
    public Guid Id { get; set; }
    public required DataSourceType DataSourceType { get; set; }
    public required string DataSourceValue { get; set; }
    public required DateTime CreatedDate { get; set; }
    public required string CollectionName { get; set; }
    public ICollection<Assistant> Assistants { get; set; } = [];
}
```

## API Endpoints Used

| Method | Endpoint | Purpose | Request Body | Response |
|--------|----------|---------|--------------|----------|
| GET | `/assistants` | List all assistants | - | Array of assistants |
| POST | `/assistants` | Create assistant | `{ name, dataSources[] }` | Created assistant |
| POST | `/assistants/{id}` | Query assistant | JSON string (question) | `{ response }` |
| GET | `/datasources` | List data sources | - | Array of data sources |
| POST | `/datasources` | Upload files | multipart/form-data | Array of collection names |
| DELETE | `/datasources/{id}` | Delete data source | - | 200 OK |

## Configuration

### API Base URL
Configured in `lib/config.ts`:
```typescript
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5067"
```

Can be overridden with environment variable:
```env
NEXT_PUBLIC_API_URL=http://your-api-url
```

## Error Handling

All API calls include:
1. **Try-catch blocks** for network errors
2. **Response status checks** (`response.ok`)
3. **User-friendly error messages** displayed in the UI
4. **Console logging** for debugging
5. **Loading states** during API calls
6. **Retry functionality** where appropriate

## Static Export Considerations

The app uses `output: 'export'` for static site generation:

1. **No Dynamic Routes**: All routes are static, IDs passed via query strings
2. **Client Components**: API calls happen client-side after page load
3. **No SSR**: All data fetching is client-side (CSR)
4. **Query Parameters**: Assistant IDs passed as `?id=xxx` in the URL

### Query String Approach

Instead of dynamic routes like `/assistants/[id]/chat`, the app uses:
- `/assistants/chat?id=123` - Chat with assistant
- `/assistants/edit?id=123` - Edit assistant

**Benefits**:
- No need for `generateStaticParams()`
- True static export - all pages pre-generated at build time
- Simpler deployment - no server-side rendering required
- Works perfectly with static hosting (Netlify, Vercel, GitHub Pages, etc.)

## Testing the Integration

1. **Start the backend API**:
   ```bash
   cd src/api
   dotnet run
   ```

2. **Start the web app**:
   ```bash
   cd src/web
   npm run dev
   ```

3. **Test flow**:
   - Upload documents via Data Sources → Create
   - Create an assistant and connect data sources
   - Chat with the assistant
   - View all assistants and data sources

## Future Enhancements

Potential improvements:
1. Add edit functionality for assistants
2. Implement data source editing
3. Add pagination for large lists
4. Add search/filter functionality
5. Implement real-time updates (WebSocket)
6. Add assistant analytics/usage stats
7. Implement user authentication
8. Add file preview before upload
9. Support for more data source types (GitHub, URL, Sitemap)
10. Batch operations (delete multiple, etc.)

