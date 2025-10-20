"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { MessageSquare, Pencil, Plus, FileText, Loader2, Database } from "lucide-react"
import { ThemeToggle } from "@/components/theme-toggle"
import { getAssistants, type Assistant } from "@/lib/api"

export default function DashboardPage() {
  const [assistants, setAssistants] = useState<Assistant[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchAssistants()
  }, [])

  const fetchAssistants = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const data = await getAssistants()
      setAssistants(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : "An error occurred")
      console.error("Error fetching assistants:", err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b border-border bg-card">
        <div className="container mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-mono font-bold text-foreground">Assistants</h1>
              <p className="text-sm text-muted-foreground mt-1">Manage your AI assistants</p>
            </div>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              <Link href="/assistants/create">
                <Button className="gap-2">
                  <Plus className="h-4 w-4" />
                  Create Assistant
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-6 py-8">
        {/* Loading State */}
        {isLoading && (
          <Card className="p-12 border border-border bg-card text-center">
            <Loader2 className="h-12 w-12 text-muted-foreground mx-auto mb-4 animate-spin" />
            <p className="text-muted-foreground">Loading assistants...</p>
          </Card>
        )}

        {/* Error State */}
        {error && !isLoading && (
          <Card className="p-12 border border-destructive bg-card text-center">
            <p className="text-destructive mb-4">{error}</p>
            <Button onClick={fetchAssistants} variant="outline">
              Try Again
            </Button>
          </Card>
        )}

        {/* Empty State */}
        {!isLoading && !error && assistants.length === 0 && (
          <Card className="p-12 border border-border bg-card text-center">
            <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-foreground mb-2">No assistants yet</h3>
            <p className="text-muted-foreground">Create your first RAG assistant to get started</p>
          </Card>
        )}

        {/* Assistants List */}
        {!isLoading && !error && assistants.length > 0 && (
          <div className="grid grid-cols-1 gap-4">
            {assistants.map((assistant) => (
              <Card
                key={assistant.id}
                className="p-6 border border-border bg-card hover:border-primary/50 transition-colors"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <h3 className="text-lg font-mono font-semibold text-foreground mb-2">{assistant.name}</h3>
                    <div className="flex items-center gap-6 text-xs text-muted-foreground font-mono mb-2">
                      <div className="flex items-center gap-2">
                        <Database className="h-3 w-3" />
                        <span>
                          {assistant.dataSources.length}{" "}
                          {assistant.dataSources.length === 1 ? "data source" : "data sources"}
                        </span>
                      </div>
                    </div>
                    <div className="text-xs text-muted-foreground/70">
                      <span>ID: {assistant.id}</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 ml-4">
                    <Link href={`/assistants/chat?id=${assistant.id}`}>
                      <Button variant="outline" size="sm" className="gap-2 bg-transparent">
                        <MessageSquare className="h-4 w-4" />
                        Chat
                      </Button>
                    </Link>
                    <Link href={`/assistants/edit?id=${assistant.id}`}>
                      <Button variant="outline" size="sm" className="gap-2 bg-transparent">
                        <Pencil className="h-4 w-4" />
                        Edit
                      </Button>
                    </Link>
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
