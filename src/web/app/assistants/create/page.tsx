"use client"

import type React from "react"

import { useState, useEffect } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import { Textarea } from "@/components/ui/textarea"
import { ArrowLeft, Database, Loader2 } from "lucide-react"

import type { DataSource } from "@/lib/api"
import { getDataSourceTypeLabel } from "@/lib/api"

export default function CreateAssistantPage() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [instructions, setInstructions] = useState("")
  const [queryResultsLimit, setQueryResultsLimit] = useState<number>(3)
  const [selectedDataSources, setSelectedDataSources] = useState<string[]>([])
  const [dataSources, setDataSources] = useState<DataSource[]>([])
  const [isLoadingDataSources, setIsLoadingDataSources] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchDataSources()
  }, [])

  const fetchDataSources = async () => {
    try {
      setIsLoadingDataSources(true)
      setError(null)
      const { getDataSources } = await import("@/lib/api")
      const data = await getDataSources()
      setDataSources(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load data sources")
      console.error("Error fetching data sources:", err)
    } finally {
      setIsLoadingDataSources(false)
    }
  }

  const toggleDataSource = (id: string) => {
    setSelectedDataSources((prev) => (prev.includes(id) ? prev.filter((dsId) => dsId !== id) : [...prev, id]))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!name || selectedDataSources.length === 0) {
      return
    }

    try {
      setIsSubmitting(true)
      setError(null)

      const { createAssistant } = await import("@/lib/api")
      const result = await createAssistant({
        name,
        dataSources: selectedDataSources,
        instructions: instructions.trim() || undefined,
        queryResultsLimit: queryResultsLimit,
      })
      
      console.log("Assistant created:", result)
      
      // Redirect to dashboard after creation
      router.push("/")
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create assistant")
      console.error("Error creating assistant:", err)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b border-border bg-card">
        <div className="container mx-auto px-6 py-4">
          <div className="flex items-center gap-4">
            <Link href="/">
              <Button variant="ghost" size="sm" className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Back
              </Button>
            </Link>
            <div>
              <h1 className="text-2xl font-mono font-bold text-foreground">Create Assistant</h1>
              <p className="text-sm text-muted-foreground mt-1">Set up a new RAG assistant</p>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-6 py-8 max-w-3xl">
        <form onSubmit={handleSubmit}>
          <Card className="p-8 border border-border bg-card">
            <div className="space-y-6">
              {/* Name Input */}
              <div className="space-y-2">
                <Label htmlFor="name" className="text-sm font-medium text-foreground">
                  Assistant Name
                </Label>
                <Input
                  id="name"
                  type="text"
                  placeholder="e.g., Product Documentation Assistant"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                  disabled={isSubmitting}
                  className="bg-background border-border text-foreground"
                />
                <p className="text-xs text-muted-foreground">Choose a descriptive name for your assistant</p>
              </div>

              {/* Special Instructions */}
              <div className="space-y-2">
                <Label htmlFor="instructions" className="text-sm font-medium text-foreground">
                  Special Instructions <span className="text-muted-foreground font-normal">(Optional)</span>
                </Label>
                <Textarea
                  id="instructions"
                  placeholder="e.g., Always provide code examples, focus on best practices, use a friendly tone..."
                  value={instructions}
                  onChange={(e) => setInstructions(e.target.value)}
                  disabled={isSubmitting}
                  className="bg-background border-border text-foreground min-h-[100px]"
                />
                <p className="text-xs text-muted-foreground">
                  Provide specific instructions to guide the assistant's responses
                </p>
              </div>

              {/* Query Results Limit */}
              <div className="space-y-2">
                <Label htmlFor="queryResultsLimit" className="text-sm font-medium text-foreground">
                  Search Results Limit
                </Label>
                <Input
                  id="queryResultsLimit"
                  type="number"
                  min="1"
                  max="20"
                  value={queryResultsLimit}
                  onChange={(e) => setQueryResultsLimit(Number(e.target.value))}
                  disabled={isSubmitting}
                  className="bg-background border-border text-foreground"
                />
                <p className="text-xs text-muted-foreground">
                  Number of relevant documents to retrieve for each query (default: 3)
                </p>
              </div>

              {/* Data Sources */}
              <div className="space-y-2">
                <Label className="text-sm font-medium text-foreground">Data Sources</Label>
                <p className="text-xs text-muted-foreground mb-4">
                  Select one or more data sources to connect with this assistant
                </p>

                {/* Loading State */}
                {isLoadingDataSources && (
                  <div className="border border-border bg-background p-8 text-center">
                    <Loader2 className="h-8 w-8 text-muted-foreground mx-auto mb-3 animate-spin" />
                    <p className="text-sm text-muted-foreground">Loading data sources...</p>
                  </div>
                )}

                {/* Error State */}
                {error && !isLoadingDataSources && (
                  <div className="border border-destructive bg-background p-8 text-center">
                    <p className="text-sm text-destructive mb-3">{error}</p>
                    <Button type="button" size="sm" variant="outline" onClick={fetchDataSources}>
                      Try Again
                    </Button>
                  </div>
                )}

                {/* Empty State */}
                {!isLoadingDataSources && !error && dataSources.length === 0 && (
                  <div className="border border-border bg-background p-8 text-center">
                    <Database className="h-8 w-8 text-muted-foreground mx-auto mb-3" />
                    <p className="text-sm text-foreground mb-1">No data sources available</p>
                    <p className="text-xs text-muted-foreground mb-4">Create a data source first to continue</p>
                    <Link href="/data-sources/create">
                      <Button type="button" size="sm">
                        Create Data Source
                      </Button>
                    </Link>
                  </div>
                )}

                {/* Data Sources List */}
                {!isLoadingDataSources && !error && dataSources.length > 0 && (
                  <div className="max-h-[300px] overflow-y-auto border border-border">
                    <div className="space-y-3 p-3">
                      {dataSources.map((dataSource) => (
                        <label
                          key={dataSource.id}
                          htmlFor={`ds-${dataSource.id}`}
                          className="flex items-start gap-3 p-4 border border-border bg-background hover:bg-secondary/50 transition-colors cursor-pointer"
                        >
                          <Checkbox
                            id={`ds-${dataSource.id}`}
                            checked={selectedDataSources.includes(dataSource.id)}
                            onCheckedChange={() => !isSubmitting && toggleDataSource(dataSource.id)}
                            disabled={isSubmitting}
                            className="mt-1"
                          />
                          <div className="flex-1 pointer-events-none">
                            <div className="flex items-center gap-2 mb-1">
                              <span className="text-sm font-medium text-foreground">
                                {dataSource.name}
                              </span>
                              <span className="text-xs font-mono px-2 py-0.5 bg-primary/10 text-primary border border-primary/20">
                                {getDataSourceTypeLabel(dataSource.dataSourceType)}
                              </span>
                            </div>
                            <p className="text-xs text-muted-foreground break-all">{dataSource.dataSourceValue}</p>
                          </div>
                        </label>
                      ))}
                    </div>
                  </div>
                )}
              </div>

              {/* Submit Error */}
              {error && !isLoadingDataSources && (
                <div className="p-3 border border-destructive bg-destructive/10 text-destructive text-sm">
                  {error}
                </div>
              )}
            </div>
          </Card>

          {/* Actions */}
          <div className="flex items-center justify-end gap-3 mt-6">
            <Link href="/">
              <Button type="button" variant="outline" disabled={isSubmitting}>
                Cancel
              </Button>
            </Link>
            <Button 
              type="submit" 
              disabled={!name || selectedDataSources.length === 0 || isSubmitting}
              className="gap-2"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />}
              {isSubmitting ? "Creating..." : "Create Assistant"}
            </Button>
          </div>
        </form>
      </main>
    </div>
  )
}
