# Application Routes

## Overview

This document describes all routes in the RAG web application and how to use them.

## Route Structure

All routes use static paths with query strings for dynamic data.

## Available Routes

### Home / Dashboard
- **URL**: `/`
- **Purpose**: List all assistants
- **Query Params**: None
- **API Calls**: `GET /assistants`

### Assistants

#### Create Assistant
- **URL**: `/assistants/create`
- **Purpose**: Create a new assistant
- **Query Params**: None
- **API Calls**: 
  - `GET /datasources` (to list available data sources)
  - `POST /assistants` (to create the assistant)

#### Chat with Assistant
- **URL**: `/assistants/chat?id={assistantId}`
- **Purpose**: Chat interface for a specific assistant
- **Query Params**: 
  - `id` (required) - The assistant's GUID
- **API Calls**:
  - `GET /assistants` (to fetch assistant details)
  - `POST /assistants/{id}` (to send chat messages)
- **Example**: `/assistants/chat?id=550e8400-e29b-41d4-a716-446655440000`

#### Edit Assistant
- **URL**: `/assistants/edit?id={assistantId}`
- **Purpose**: Edit assistant configuration
- **Query Params**: 
  - `id` (required) - The assistant's GUID
- **API Calls**: TBD (currently mock data)
- **Example**: `/assistants/edit?id=550e8400-e29b-41d4-a716-446655440000`

### Data Sources

#### List Data Sources
- **URL**: `/data-sources`
- **Purpose**: View all data sources
- **Query Params**: None
- **API Calls**: 
  - `GET /datasources`
  - `DELETE /datasources/{id}` (when deleting)

#### Create Data Source
- **URL**: `/data-sources/create`
- **Purpose**: Upload files to create new data sources
- **Query Params**: None
- **API Calls**: `POST /datasources` (multipart/form-data)

## Query String Parameters

### Reading Query Parameters

In client components, use `useSearchParams()`:

```typescript
"use client"

import { useSearchParams } from "next/navigation"
import { Suspense } from "react"

function MyPageContent() {
  const searchParams = useSearchParams()
  const id = searchParams.get("id")
  
  if (!id) {
    return <div>ID is required</div>
  }
  
  return <div>ID: {id}</div>
}

export default function MyPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <MyPageContent />
    </Suspense>
  )
}
```

### Creating Links with Query Parameters

```typescript
import Link from "next/link"

// Simple query string
<Link href={`/assistants/chat?id=${assistantId}`}>
  Chat
</Link>

// Multiple parameters
<Link href={`/assistants/chat?id=${assistantId}&mode=debug`}>
  Debug Chat
</Link>

// Using URLSearchParams for complex queries
const params = new URLSearchParams({
  id: assistantId,
  tab: "settings",
  view: "advanced"
})
<Link href={`/assistants/edit?${params.toString()}`}>
  Advanced Settings
</Link>
```

## Navigation Flow

### Typical User Journey

1. **Start**: User lands on `/` (home page)
2. **View Assistants**: Sees list of all assistants
3. **Option A - Create New**:
   - Click "Create Assistant" → `/assistants/create`
   - Select data sources
   - Submit → Returns to `/`
4. **Option B - Use Existing**:
   - Click "Chat" on an assistant → `/assistants/chat?id=xxx`
   - Send messages to the assistant
   - Click "Back" → Returns to `/`
5. **Option C - Edit**:
   - Click "Edit" on an assistant → `/assistants/edit?id=xxx`
   - Modify settings
   - Save → Returns to `/`
6. **Manage Data Sources**:
   - Click "Data Sources" → `/data-sources`
   - Click "Create Data Source" → `/data-sources/create`
   - Upload files → Returns to `/data-sources`

## Error Handling

### Missing Query Parameters

Pages that require query parameters will show an error message:

```typescript
if (!id) {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center">
      <div className="text-center">
        <h1 className="text-2xl font-bold text-foreground mb-2">
          Assistant ID Required
        </h1>
        <p className="text-muted-foreground">
          Please provide an assistant ID in the URL.
        </p>
      </div>
    </div>
  )
}
```

### Invalid IDs

If an ID doesn't exist, the API will return a 404, and the page will show an error message.

## URL Examples

### Valid URLs
```
✅ /
✅ /assistants/create
✅ /assistants/chat?id=550e8400-e29b-41d4-a716-446655440000
✅ /assistants/edit?id=550e8400-e29b-41d4-a716-446655440000
✅ /data-sources
✅ /data-sources/create
```

### Invalid URLs (will show error)
```
❌ /assistants/chat (missing id parameter)
❌ /assistants/edit (missing id parameter)
❌ /assistants/550e8400-e29b-41d4-a716-446655440000/chat (old format)
```

## Adding New Routes

### Static Route (no parameters)

1. Create `app/my-page/page.tsx`
2. Add your component
3. Link to it: `<Link href="/my-page">My Page</Link>`

### Route with Query Parameters

1. Create `app/my-page/page.tsx` with `"use client"`
2. Use `useSearchParams()` to read parameters
3. Wrap in `<Suspense>` boundary
4. Validate required parameters
5. Link to it: `<Link href="/my-page?id=123">My Page</Link>`

## Best Practices

1. **Always validate query parameters** - Check if required params exist
2. **Use Suspense boundaries** - Wrap components using `useSearchParams()`
3. **Provide fallback UI** - Show loading states and error messages
4. **Use type-safe IDs** - Validate GUID format if needed
5. **Encode special characters** - Use `encodeURIComponent()` for param values
6. **Keep URLs clean** - Only include necessary parameters
7. **Document new routes** - Update this file when adding routes

## SEO Considerations

Since this is a static export with client-side routing:
- All pages are pre-rendered at build time
- Query parameters are read client-side
- No SSR or dynamic meta tags for specific assistants
- Consider adding a sitemap if needed for search engines

## Future Enhancements

Potential improvements to routing:
1. Add breadcrumbs for navigation
2. Implement browser back/forward handling
3. Add route guards for authentication
4. Implement deep linking support
5. Add analytics tracking for route changes

