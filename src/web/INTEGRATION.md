# API Integration Summary

This document describes how the Next.js web application integrates with the backend API.

## Overview

The web application is now fully integrated with the C# backend API running on `http://localhost:5067`. All pages fetch real data from the API and submit changes back to it.

### Key Features

✅ **Complete CRUD Operations**
- Create, Read, Update, and Delete assistants
- Create, Read, and Delete data sources
- Full form validation and error handling

✅ **Modern UI/UX**
- Dark/Light theme support
- Responsive design with shadcn/ui components
- Loading states, empty states, and error states
- Confirmation dialogs for destructive actions

✅ **Real-time Chat**
- Interactive chat interface with assistants
- Message history and timestamps
- Loading indicators and error handling
- Auto-scrolling messages

✅ **Embeddable Widget**
- Standalone JavaScript chat widget
- Easy integration into any website
- Self-contained with inline styles

✅ **Static Export Ready**
- Query string-based routing (no dynamic routes)
- Client-side data fetching
- Deployable to any static hosting service

✅ **TypeScript & Type Safety**
- Fully typed API clients
- Type-safe request/response models
- IntelliSense support throughout

## Integrated Pages

### 1. Home Page (Assistants List)
**File**: `app/page.tsx`

**Features**:
- Fetches all assistants from `GET /assistants`
- Displays assistant name, ID, and connected data sources count
- Links to chat and edit pages
- Loading and error states
- Theme toggle support
- Empty state with call-to-action

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
- Allows selecting multiple data sources via checkboxes
- Displays data source name and type badges
- Submits to `POST /assistants` with:
  ```json
  {
    "name": "Assistant Name",
    "dataSources": ["guid1", "guid2"]
  }
  ```
- Loading states for data sources
- Empty state when no data sources exist
- Redirects to home page on success
- Form validation (requires name and at least one data source)

### 3. Chat Page
**File**: `app/assistants/chat/page.tsx`
**URL**: `/assistants/chat?id={assistantId}`

**Features**:
- Uses Suspense for loading states
- Fetches assistant details via `GET /assistants/{id}` to display name
- Sends user messages to `POST /assistants/{id}`
- Request body: JSON string of the user's question
- Response: `{ "response": "AI answer" }`
- Real-time chat interface with message history
- Auto-scrolling to latest message
- Timestamps for all messages
- Visual distinction between user and assistant messages
- "Thinking..." indicator during API calls
- Error handling with user-friendly messages

### 4. Edit Assistant Page
**File**: `app/assistants/edit/page.tsx`
**URL**: `/assistants/edit?id={assistantId}`

**Features**:
- Fetches assistant details via `GET /assistants/{id}`
- Pre-populates form with existing data
- Allows updating name and data sources
- Updates via `PUT /assistants/{id}`
- Delete functionality with confirmation dialog via `DELETE /assistants/{id}`
- Loading states for both assistant and data sources
- Suspense wrapper for progressive loading
- Redirects to home page on success or after deletion

### 5. Data Sources List
**File**: `app/data-sources/page.tsx`

**Features**:
- Fetches all data sources from `GET /datasources`
- Displays data source name, type, value, ID, and creation date
- Delete functionality via `DELETE /datasources/{id}` with confirmation dialog
- Auto-refreshes list after deletion
- Loading and error states
- Empty state with call-to-action
- Theme toggle support
- Formatted dates (e.g., "Jan 15, 2024")

**API Response Model**:
```typescript
{
  id: string
  name: string
  dataSourceType: DataSourceType  // 0=Stream, 1=File, 2=GitHub, 3=Url, 4=Sitemap
  dataSourceValue: string
  createdDate: string
  collectionName: string
}
```

### 6. Create Data Source Page
**File**: `app/data-sources/create/page.tsx`

**Features**:
- Name input for the data source
- File upload interface with drag-and-drop support
- Multi-file selection
- File preview with name and size
- Individual file removal before upload
- Submits via `POST /datasources` as multipart/form-data with `name` field
- Backend processes each file and creates data source entries
- Shows upload progress and success/error messages
- Redirects to data sources list on success
- Accepts: .pdf, .txt, .md, .doc, .docx files

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
    public required string Name { get; set; }
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
| GET | `/assistants` | List all assistants | - | Array of assistants with `{ id, name, dataSources[] }` |
| GET | `/assistants/{id}` | Get single assistant | - | `{ id, name, dataSources[] }` |
| POST | `/assistants` | Create assistant | `{ name, dataSources[] }` | `{ id, name }` |
| PUT | `/assistants/{id}` | Update assistant | `{ name, dataSources[] }` | 200 OK |
| DELETE | `/assistants/{id}` | Delete assistant | - | 200 OK |
| POST | `/assistants/{id}` | Query assistant | JSON string (question) | `{ response }` |
| POST | `/assistants/all` | Query all data sources | JSON string (question) | `{ response }` |
| GET | `/datasources` | List data sources | - | Array of data sources |
| POST | `/datasources` | Upload files | multipart/form-data (files + name) | Array of collection names |
| DELETE | `/datasources/{id}` | Delete data source | - | 200 OK |
| GET | `/assistants/{id}/chat.js` | Get chat widget script | - | JavaScript file with embedded assistant ID |

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

All API calls include comprehensive error handling:

1. **Try-catch blocks** for network errors
   - Wraps all async API calls
   - Catches network failures and timeout errors

2. **Response status checks** (`response.ok`)
   - Validates HTTP status codes
   - Throws descriptive errors for non-200 responses

3. **User-friendly error messages**
   - Displayed in colored alert boxes in the UI
   - Generic messages for technical errors
   - Specific guidance for recoverable errors

4. **Console logging** for debugging
   - All errors logged to console with context
   - Includes full error objects for debugging

5. **Loading states** during API calls
   - Buttons disabled during operations
   - Loading spinners shown
   - Input fields disabled during submission

6. **Retry functionality** where appropriate
   - "Try Again" buttons on error states
   - Maintains user's context (form data preserved)

7. **Validation errors**
   - Client-side form validation before submission
   - Required field indicators
   - Descriptive validation messages

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

## UI/UX Features

The web application includes a modern, polished user interface with:

**Design System**:
- Built with shadcn/ui components
- Dark/Light theme support with theme toggle
- Consistent typography with monospace fonts for IDs and technical elements
- Gradient color schemes and smooth transitions
- Responsive layouts optimized for desktop

**User Experience**:
- Loading skeletons and spinners for async operations
- Empty states with helpful call-to-action buttons
- Confirmation dialogs for destructive actions (delete)
- Error boundaries with retry functionality
- Form validation with visual feedback
- Toast notifications (via shadcn/ui)
- Accessible components (keyboard navigation, ARIA labels)

**Visual Indicators**:
- Color-coded badges for data source types
- Icons from Lucide React for visual clarity
- Hover states and transitions for interactive elements
- Card-based layouts for content organization
- Status indicators (loading, success, error)

**Navigation**:
- Breadcrumb-style back buttons
- Consistent header structure across pages
- Link-based navigation (no full page reloads)
- Query string-based routing for dynamic pages

## Testing the Integration

### Development Setup

1. **Start the backend API**:
   ```bash
   cd src/api
   dotnet run
   ```
   API will be available at `http://localhost:5067`

2. **Start the web app** (development mode):
   ```bash
   cd src/web
   npm run dev
   ```
   Web app will be available at `http://localhost:3000`

3. **Build static export** (production):
   ```bash
   cd src/web
   npm run build
   ```
   Static files will be generated in the `out/` directory

### Test Flow

1. **Create Data Sources**:
   - Navigate to Data Sources → Create
   - Enter a name (e.g., "Product Docs")
   - Upload one or more documents
   - Verify success and redirection to data sources list

2. **Create Assistant**:
   - Navigate to Assistants → Create
   - Enter assistant name
   - Select one or more data sources
   - Verify success and redirection to home

3. **Chat with Assistant**:
   - Click "Chat" button on any assistant
   - Ask questions about the uploaded documents
   - Verify responses are contextually relevant

4. **Edit Assistant**:
   - Click "Edit" button on any assistant
   - Modify name or data sources
   - Verify changes are saved

5. **Delete Operations**:
   - Test delete functionality for assistants and data sources
   - Verify confirmation dialogs appear
   - Confirm deletion and verify item is removed

6. **Test Chat Widget**:
   - Create a test HTML file
   - Add script tag: `<script src="http://localhost:5067/assistants/{id}/chat.js"></script>`
   - Open in browser and test chat functionality

## Chat Widget Integration

The API provides an embeddable chat widget that can be integrated into any website.

**File**: `widget/js/widget.js`
**Endpoint**: `GET /assistants/{id}/chat.js`

**Features**:
- Self-contained JavaScript widget with inline CSS
- Floating chat button in bottom-right corner
- Expandable chat interface
- Styled with gradient purple theme
- Real-time communication with assistant
- Loading indicators during API calls
- Error handling
- Auto-scrolling messages
- Template variables replaced server-side: `{{ASSISTANT_ID}}` and `{{API_BASE_URL}}`

**Integration**:
```html
<!-- Add to any webpage -->
<script src="http://localhost:5067/assistants/{assistant-id}/chat.js"></script>
```

The widget automatically:
1. Injects required CSS styles
2. Creates chat UI elements
3. Handles user interactions
4. Makes API calls to the assistant endpoint
5. Displays responses in a user-friendly interface

## API Client Architecture

The frontend uses a centralized API client architecture in `lib/api/`:

**Structure**:
- `assistants.ts` - Assistants API client with types and convenience functions
- `datasources.ts` - Data Sources API client with types and convenience functions  
- `index.ts` - Central export point for all API functionality

**Features**:
- Class-based API clients (`AssistantsApiClient`, `DataSourcesApiClient`)
- Singleton instances exported for convenience
- Standalone convenience functions for direct imports
- TypeScript types for all requests and responses
- Consistent error handling
- Configurable base URL via environment variable

**Example Usage**:
```typescript
import { getAssistants, createAssistant, queryAssistant, type Assistant } from '@/lib/api'

// Fetch all assistants
const assistants = await getAssistants()

// Create new assistant
const newAssistant = await createAssistant({ 
  name: 'My Assistant', 
  dataSources: ['id1', 'id2'] 
})

// Query an assistant
const response = await queryAssistant('assistant-id', 'What is the product price?')
console.log(response.response) // AI-generated answer

// Update an assistant
await updateAssistant('assistant-id', { 
  name: 'Updated Name', 
  dataSources: ['id3'] 
})

// Delete an assistant
await deleteAssistant('assistant-id')
```

## Deployment

### Backend API Deployment

The C# backend can be deployed using:

1. **Docker** (recommended):
   ```bash
   cd src/api
   docker build -t rag-api .
   docker run -p 5067:8080 rag-api
   ```

2. **Direct deployment**:
   ```bash
   cd src/api
   dotnet publish -c Release
   # Deploy the published files to your server
   ```

**Environment Configuration**:
- Database connection strings
- Qdrant vector storage connection
- Ollama/OpenAI API endpoints
- CORS settings for frontend domains

### Frontend Deployment

The Next.js app generates a static export that can be hosted anywhere:

1. **Build the static export**:
   ```bash
   cd src/web
   npm run build
   ```
   Output: `out/` directory

2. **Deploy to static hosting**:
   - **Netlify**: Drag `out/` folder to Netlify dashboard
   - **Vercel**: Connect GitHub repo or upload `out/` directory
   - **GitHub Pages**: Push `out/` contents to `gh-pages` branch
   - **AWS S3**: Upload `out/` contents to S3 bucket with static hosting
   - **Any web server**: Serve `out/` directory with any HTTP server

3. **Configure API URL**:
   Set environment variable before build:
   ```bash
   NEXT_PUBLIC_API_URL=https://your-api-domain.com npm run build
   ```

### Widget Deployment

The chat widget is served directly from the API at `/assistants/{id}/chat.js`:

```html
<script src="https://your-api-domain.com/assistants/{assistant-id}/chat.js"></script>
```

The widget automatically uses the correct API URL (injected server-side during rendering).