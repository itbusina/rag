# RAG Web Application

A modern Next.js web application for managing RAG (Retrieval-Augmented Generation) assistants and data sources.

## Features

- 📚 **Data Sources Management**: Upload and manage document collections with support for PDF, TXT, MD, DOC, DOCX files
- 🤖 **Assistants**: Create, edit, and delete AI assistants connected to specific data sources
- 💬 **Chat Interface**: Interactive chat with your RAG assistants featuring real-time responses
- 🎨 **Modern UI**: Beautiful, responsive interface with dark/light mode support built on shadcn/ui
- 🚀 **Static Export**: Can be deployed as a static site to any hosting service
- 🔧 **Full CRUD Operations**: Complete create, read, update, delete functionality for all entities
- 🎯 **Type-Safe API**: Fully typed TypeScript API client with IntelliSense support
- 📱 **Embeddable Widget**: JavaScript chat widget that can be embedded on any website
- 🔄 **Loading & Error States**: Comprehensive UI feedback for all operations
- ✅ **Form Validation**: Client-side validation with user-friendly error messages
- 🗑️ **Confirmation Dialogs**: Safety prompts for destructive actions

## Prerequisites

- Node.js 18+ 
- The backend API running on `http://localhost:5067` (or configure via environment variable)

## Getting Started

### Installation

```bash
npm install
```

### Development

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) to view the application.

### Build

```bash
npm run build
```

This creates a static export in the `out/` directory that can be deployed to any static hosting service.

### Preview Production Build

```bash
npm run start
```

## Configuration

### Environment Variables

Create a `.env.local` file in the root of the web directory:

```env
# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:5067
```

## Project Structure

```
src/web/
├── app/                          # Next.js app directory
│   ├── page.tsx                 # Home page (Assistants list)
│   ├── layout.tsx               # Root layout with theme provider
│   ├── globals.css              # Global styles and CSS variables
│   ├── assistants/
│   │   ├── create/
│   │   │   └── page.tsx         # Create assistant page
│   │   ├── chat/
│   │   │   └── page.tsx         # Chat interface (query string based)
│   │   └── edit/
│   │       └── page.tsx         # Edit assistant (query string based)
│   └── data-sources/
│       ├── page.tsx             # Data sources list
│       └── create/
│           └── page.tsx         # Upload documents
├── components/                   # Reusable UI components
│   ├── ui/                      # shadcn/ui components (57 components)
│   ├── app-layout.tsx           # Application layout wrapper
│   ├── sidebar-nav.tsx          # Sidebar navigation component
│   ├── theme-provider.tsx       # Theme context provider
│   └── theme-toggle.tsx         # Dark/light mode toggle
├── hooks/                        # Custom React hooks
│   ├── use-mobile.ts            # Mobile detection hook
│   └── use-toast.ts             # Toast notifications hook
├── lib/
│   ├── api/                     # Centralized API client
│   │   ├── index.ts             # Main exports
│   │   ├── assistants.ts        # Assistants API client
│   │   ├── datasources.ts       # Data sources API client
│   │   └── README.md            # API client documentation
│   ├── config.ts                # API configuration
│   └── utils.ts                 # Utility functions (cn, etc.)
├── public/                       # Static assets
│   ├── placeholder-logo.svg
│   ├── placeholder-user.jpg
│   └── placeholder.svg
├── out/                          # Static export output (after build)
├── components.json               # shadcn/ui configuration
├── next.config.mjs               # Next.js configuration
├── tailwind.config.ts            # Tailwind CSS configuration
├── tsconfig.json                 # TypeScript configuration
├── INTEGRATION.md                # API integration documentation
└── README.md                     # This file

```

## API Integration

The application uses a centralized API client located in `lib/api/`. This provides:

- **Type-safe API calls** with TypeScript interfaces
- **Consistent error handling** across all requests
- **Single source of truth** for API endpoints
- **Easy mocking** for tests

### Using the API Client

```typescript
import { 
  getAssistants, 
  getAssistant,
  createAssistant, 
  updateAssistant,
  deleteAssistant,
  queryAssistant,
  getDataSources,
  createDataSources,
  deleteDataSource 
} from "@/lib/api"

// Fetch all assistants
const assistants = await getAssistants()

// Get a single assistant
const assistant = await getAssistant("assistant-id")

// Create an assistant
const result = await createAssistant({
  name: "My Assistant",
  dataSources: ["datasource-id-1", "datasource-id-2"]
})

// Update an assistant
await updateAssistant("assistant-id", {
  name: "Updated Name",
  dataSources: ["new-datasource-id"]
})

// Delete an assistant
await deleteAssistant("assistant-id")

// Query an assistant
const response = await queryAssistant("assistant-id", "What is X?")
console.log(response.response)

// Get all data sources
const dataSources = await getDataSources()

// Create data sources from files
const files = [...] // File[] from input
const collections = await createDataSources(files, "My Data Source")

// Delete a data source
await deleteDataSource("datasource-id")
```

**Error Handling**:
All API functions throw errors on failure. Wrap calls in try-catch:

```typescript
try {
  const assistants = await getAssistants()
  setAssistants(assistants)
} catch (error) {
  console.error("Failed to fetch assistants:", error)
  setError(error.message)
}
```

See `lib/api/README.md` for complete API client documentation.

### Backend Endpoints

**Assistants**:
- `GET /assistants` - List all assistants
- `GET /assistants/{id}` - Get a single assistant
- `POST /assistants` - Create a new assistant
- `PUT /assistants/{id}` - Update an assistant
- `DELETE /assistants/{id}` - Delete an assistant
- `POST /assistants/{id}` - Query an assistant with a question
- `POST /assistants/all` - Query all data sources
- `GET /assistants/{id}/chat.js` - Get embeddable chat widget script

**Data Sources**:
- `GET /datasources` - List all data sources
- `POST /datasources` - Upload files to create data sources (multipart/form-data with name field)
- `DELETE /datasources/{id}` - Delete a data source

See `INTEGRATION.md` for detailed API documentation.

## Technologies

- **Next.js 15** - React framework with App Router
- **TypeScript** - Type safety
- **Tailwind CSS** - Styling
- **shadcn/ui** - UI component library
- **Lucide React** - Icons

## Pages & Routes

The application includes 6 main pages:

1. **Home (`/`)** - List of all assistants with create, edit, and chat actions
2. **Create Assistant (`/assistants/create`)** - Form to create new assistants and select data sources
3. **Edit Assistant (`/assistants/edit?id={id}`)** - Edit assistant name, data sources, or delete assistant
4. **Chat (`/assistants/chat?id={id}`)** - Interactive chat interface with an assistant
5. **Data Sources (`/data-sources`)** - List of all data sources with delete functionality
6. **Create Data Source (`/data-sources/create`)** - Upload documents to create new data sources

All pages feature:
- Loading states with spinners
- Empty states with helpful CTAs
- Error handling with retry options
- Responsive layouts
- Theme support (dark/light)

## Deployment

Since the application uses `output: 'export'`, it can be deployed to:

- **Vercel** - `vercel deploy` or connect GitHub repo
- **Netlify** - Deploy the `out/` directory or connect GitHub repo
- **GitHub Pages** - Deploy the `out/` directory to gh-pages branch
- **AWS S3** - Upload `out/` directory to S3 bucket with static hosting
- **Any static hosting** - Serve the `out/` directory with any HTTP server

### Environment Configuration for Production

Set the API URL before building:

```bash
NEXT_PUBLIC_API_URL=https://your-api-domain.com npm run build
```

### Important Notes for Static Export

1. All routes are static - no dynamic `[id]` segments
2. Assistant IDs are passed via query strings (e.g., `/assistants/chat?id=123`)
3. This eliminates the need for `generateStaticParams()` and allows true static export
4. All data fetching happens client-side after page load
5. Backend API must have CORS configured to allow requests from your frontend domain

## Chat Widget

The backend provides an embeddable chat widget that can be integrated into any website:

```html
<!-- Add this script tag to any webpage -->
<script src="http://localhost:5067/assistants/{assistant-id}/chat.js"></script>
```

**Features**:
- Self-contained with inline CSS (no external dependencies)
- Floating chat button in bottom-right corner
- Expandable chat interface
- Gradient purple theme
- Real-time communication with the assistant
- Loading indicators and error handling

The widget is served dynamically from the API with the assistant ID and API URL pre-configured.

## Development Notes

### Adding New Pages

1. Create page in `app/` directory
2. Use query strings for dynamic data (e.g., `?id=123`) instead of route parameters
3. Use `"use client"` directive for pages that need client-side interactivity
4. Wrap `useSearchParams()` calls in `<Suspense>` boundaries
5. Add loading, empty, and error states
6. Use the centralized API client from `lib/api/`

### Styling

The project uses Tailwind CSS with a custom theme. Colors and styles are defined in:
- `app/globals.css` - Global styles and CSS variables
- `tailwind.config.ts` - Tailwind configuration
- Theme variables support both light and dark modes

**Color Variables**:
- `background`, `foreground` - Main background and text colors
- `primary`, `secondary` - Brand colors
- `muted`, `accent` - Supporting colors
- `destructive` - Error/delete actions
- `border` - Border colors

### Icons

Icons are from Lucide React. Import and use like:

```tsx
import { Plus, Trash2, Loader2, MessageSquare } from "lucide-react"

<Plus className="h-4 w-4" />
```

### Components

Use shadcn/ui components for consistency:
- `Button` - All buttons
- `Card` - Content containers
- `Input` - Form inputs
- `Label` - Form labels
- `AlertDialog` - Confirmation dialogs
- `Checkbox` - Multi-select options

Import from `@/components/ui/[component-name]`

## Troubleshooting

### API Connection Issues

If you see "Failed to fetch" errors:
1. Ensure the backend API is running on `http://localhost:5067`
2. Check CORS settings in the backend
3. Verify the `NEXT_PUBLIC_API_URL` environment variable
4. Check browser console for detailed error messages
5. Ensure no firewall is blocking the connection

### Query String Parameters

Pages using query strings (like `/assistants/chat?id=123`):
1. Must use `"use client"` directive
2. Use `useSearchParams()` from `next/navigation` to read parameters
3. Wrap the component using `useSearchParams()` in a `<Suspense>` boundary
4. Always validate that required parameters exist

### Build Issues

If the build fails:
1. Run `npm install` to ensure all dependencies are installed
2. Delete `.next/` and `out/` directories and rebuild
3. Check for TypeScript errors with `npm run build`
4. Ensure Node.js version is 18 or higher

### Theme Not Persisting

The theme preference is stored in localStorage and should persist across sessions. If it doesn't:
1. Check browser localStorage is enabled
2. Clear browser cache and try again
3. Check browser console for errors

## Documentation

- **`INTEGRATION.md`** - Comprehensive API integration documentation

## Contributing

When contributing to the frontend:

1. Follow the existing code style and patterns
2. Use TypeScript for all new files
3. Add proper error handling for all API calls
4. Include loading and empty states for all data fetching
5. Test both light and dark themes
6. Ensure responsive design on different screen sizes
7. Add confirmation dialogs for destructive actions
8. Use the centralized API client in `lib/api/`

## License

This project is part of the RAG system.

