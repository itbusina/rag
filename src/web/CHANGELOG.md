# Changelog

## Query String Refactoring (October 11, 2025)

### Major Changes

#### Removed Dynamic Routes
- **Deleted**: `app/assistants/[id]/chat/` directory
- **Deleted**: `app/assistants/[id]/edit/` directory
- **Reason**: Eliminated the need for `generateStaticParams()` and simplified static export

#### Added Query String-Based Routes
- **Added**: `app/assistants/chat/page.tsx` - Chat page using `?id=` query parameter
- **Added**: `app/assistants/edit/page.tsx` - Edit page using `?id=` query parameter
- **Added**: `app/assistants/chat/ChatClient.tsx` - Client component for chat
- **Added**: `app/assistants/edit/EditAssistantClient.tsx` - Client component for edit

### Technical Implementation

#### New URL Structure
- **Before**: `/assistants/[id]/chat` (dynamic route)
- **After**: `/assistants/chat?id=123` (query string)

- **Before**: `/assistants/[id]/edit` (dynamic route)
- **After**: `/assistants/edit?id=123` (query string)

#### Code Changes

1. **Chat Page** (`app/assistants/chat/page.tsx`):
   ```typescript
   - Uses useSearchParams() to read ?id= parameter
   - Wrapped in Suspense boundary for loading state
   - Validates that ID parameter exists
   - Passes ID to ChatClient component
   ```

2. **Edit Page** (`app/assistants/edit/page.tsx`):
   ```typescript
   - Uses useSearchParams() to read ?id= parameter
   - Wrapped in Suspense boundary for loading state
   - Validates that ID parameter exists
   - Passes ID to EditAssistantClient component
   ```

3. **Updated Links** (`app/AssistantsClient.tsx`):
   ```typescript
   - Chat link: `/assistants/chat?id=${assistant.id}`
   - Edit link: `/assistants/edit?id=${assistant.id}`
   ```

### Benefits

1. **No generateStaticParams() Required**
   - Eliminates complex static generation logic
   - No need to fetch all assistant IDs at build time
   - Simpler build process

2. **True Static Export**
   - All pages are truly static (○ in build output)
   - No SSG (●) pages that need special handling
   - Works with any static hosting provider

3. **Simpler Architecture**
   - No separation between server and client components for routing
   - Cleaner file structure
   - Easier to understand and maintain

4. **Better Developer Experience**
   - No build-time errors about missing generateStaticParams()
   - Faster builds (fewer pages to generate)
   - More predictable behavior

### Build Output Comparison

#### Before (with dynamic routes):
```
● /assistants/[id]/chat                3.03 kB         115 kB
├   └ /assistants/demo/chat
● /assistants/[id]/edit                3.68 kB         115 kB
├   └ /assistants/demo/edit

● = SSG (Server-Side Generated with generateStaticParams)
```

#### After (with query strings):
```
○ /assistants/chat                     3.35 kB         115 kB
○ /assistants/edit                     3.91 kB         115 kB

○ = Static (prerendered as static content)
```

### Migration Guide

If you have existing links or bookmarks:

**Old URLs** (no longer work):
- `/assistants/123/chat`
- `/assistants/456/edit`

**New URLs** (use these):
- `/assistants/chat?id=123`
- `/assistants/edit?id=456`

### Documentation Updates

- Updated `README.md` with query string approach
- Updated `INTEGRATION.md` with new URL patterns
- Removed references to `generateStaticParams()`
- Added troubleshooting section for query parameters

### Testing

All pages tested and working:
- ✅ Home page (assistants list)
- ✅ Create assistant
- ✅ Chat with assistant (via query string)
- ✅ Edit assistant (via query string)
- ✅ Data sources list
- ✅ Create data source

### API Integration

No changes to API integration - all endpoints remain the same:
- `GET /assistants` - List assistants
- `POST /assistants` - Create assistant
- `POST /assistants/{id}` - Query assistant
- `GET /datasources` - List data sources
- `POST /datasources` - Create data source
- `DELETE /datasources/{id}` - Delete data source

### Files Modified

1. `app/AssistantsClient.tsx` - Updated links to use query strings
2. `app/assistants/chat/page.tsx` - New query string-based page
3. `app/assistants/chat/ChatClient.tsx` - Moved from [id] directory
4. `app/assistants/edit/page.tsx` - New query string-based page
5. `app/assistants/edit/EditAssistantClient.tsx` - Moved from [id] directory
6. `README.md` - Updated documentation
7. `INTEGRATION.md` - Updated integration guide

### Files Deleted

1. `app/assistants/[id]/chat/page.tsx`
2. `app/assistants/[id]/chat/ChatClient.tsx`
3. `app/assistants/[id]/edit/page.tsx`
4. `app/assistants/[id]/edit/EditAssistantClient.tsx`

### Breaking Changes

⚠️ **URL Structure Changed**

If you have:
- Bookmarks to specific assistants
- External links to assistant pages
- Hardcoded URLs in other applications

You need to update them to use the new query string format.

### Future Considerations

This approach scales well because:
1. No build-time page generation needed
2. Works with unlimited number of assistants
3. No performance impact on build time
4. Compatible with all static hosting providers
5. Easy to add more query parameters if needed (e.g., `?id=123&tab=settings`)

