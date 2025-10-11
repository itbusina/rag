# RAG Web Application

A modern Next.js web application for managing RAG (Retrieval-Augmented Generation) assistants and data sources.

## Features

- 📚 **Data Sources Management**: Upload and manage document collections
- 🤖 **Assistants**: Create AI assistants connected to specific data sources
- 💬 **Chat Interface**: Interactive chat with your RAG assistants
- 🎨 **Modern UI**: Beautiful, responsive interface with dark/light mode support
- 🚀 **Static Export**: Can be deployed as a static site

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
│   ├── AssistantsClient.tsx     # Client component for assistants
│   ├── assistants/
│   │   ├── create/              # Create assistant page
│   │   ├── chat/                # Chat interface (query string based)
│   │   └── edit/                # Edit assistant (query string based)
│   └── data-sources/
│       ├── page.tsx             # Data sources list
│       ├── DataSourcesClient.tsx
│       └── create/              # Upload documents
├── components/                   # Reusable UI components
│   ├── ui/                      # shadcn/ui components
│   ├── theme-provider.tsx
│   └── theme-toggle.tsx
├── lib/
│   ├── api/                     # Centralized API client
│   │   ├── index.ts             # Main exports
│   │   ├── assistants.ts        # Assistants API
│   │   ├── datasources.ts       # Data sources API
│   │   └── README.md            # API client documentation
│   ├── config.ts                # API configuration
│   └── utils.ts                 # Utility functions
└── public/                      # Static assets

```

## API Integration

The application uses a centralized API client located in `lib/api/`. This provides:

- **Type-safe API calls** with TypeScript interfaces
- **Consistent error handling** across all requests
- **Single source of truth** for API endpoints
- **Easy mocking** for tests

### Using the API Client

```typescript
import { getAssistants, createAssistant, queryAssistant } from "@/lib/api"

// Fetch all assistants
const assistants = await getAssistants()

// Create an assistant
const result = await createAssistant({
  name: "My Assistant",
  dataSources: ["datasource-id"]
})

// Query an assistant
const response = await queryAssistant("assistant-id", "What is X?")
```

See `lib/api/README.md` for complete API client documentation.

### Backend Endpoints

- `GET /assistants` - List all assistants
- `POST /assistants` - Create a new assistant
- `POST /assistants/{id}` - Query an assistant
- `GET /datasources` - List all data sources
- `POST /datasources` - Upload files to create data sources
- `DELETE /datasources/{id}` - Delete a data source

## Technologies

- **Next.js 15** - React framework with App Router
- **TypeScript** - Type safety
- **Tailwind CSS** - Styling
- **shadcn/ui** - UI component library
- **Lucide React** - Icons

## Deployment

Since the application uses `output: 'export'`, it can be deployed to:

- **Vercel** - `vercel deploy`
- **Netlify** - Deploy the `out/` directory
- **GitHub Pages** - Deploy the `out/` directory
- **Any static hosting** - Serve the `out/` directory

### Important Notes for Static Export

1. All routes are static - no dynamic `[id]` segments
2. Assistant IDs are passed via query strings (e.g., `/assistants/chat?id=123`)
3. This eliminates the need for `generateStaticParams()` and allows true static export

## Development Notes

### Adding New Pages

1. Create page in `app/` directory
2. Use query strings for dynamic data (e.g., `?id=123`) instead of route parameters
3. Use `"use client"` directive for pages that need client-side interactivity
4. Wrap `useSearchParams()` calls in `<Suspense>` boundaries

### Styling

The project uses Tailwind CSS with a custom theme. Colors and styles are defined in:
- `app/globals.css` - Global styles and CSS variables
- `tailwind.config.ts` - Tailwind configuration

### Icons

Icons are from Lucide React. Import and use like:

```tsx
import { Plus, Trash2 } from "lucide-react"
```

## Troubleshooting

### API Connection Issues

If you see "Failed to fetch" errors:
1. Ensure the backend API is running on `http://localhost:5067`
2. Check CORS settings in the backend
3. Verify the `NEXT_PUBLIC_API_URL` environment variable

### Query String Parameters

Pages using query strings (like `/assistants/chat?id=123`):
1. Must use `"use client"` directive
2. Use `useSearchParams()` from `next/navigation` to read parameters
3. Wrap the component using `useSearchParams()` in a `<Suspense>` boundary
4. Always validate that required parameters exist

## License

This project is part of the RAG system.

