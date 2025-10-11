"use client"

import type React from "react"

import { Suspense, useState, useEffect } from "react"
import { useSearchParams, useRouter } from "next/navigation"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { ArrowLeft, Database, Loader2, Trash2 } from "lucide-react"

import type { DataSource, Assistant } from "@/lib/api"
import { getDataSourceTypeLabel } from "@/lib/api"

function EditPageContent() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const assistantId = searchParams.get("id")

  const [name, setName] = useState("")
  const [selectedDataSources, setSelectedDataSources] = useState<string[]>([])
  const [dataSources, setDataSources] = useState<DataSource[]>([])
  const [isLoadingAssistant, setIsLoadingAssistant] = useState(true)
  const [isLoadingDataSources, setIsLoadingDataSources] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [assistant, setAssistant] = useState<Assistant | null>(null)

  useEffect(() => {
    if (assistantId) {
      fetchAssistant()
      fetchDataSources()
    }
  }, [assistantId])

  const fetchAssistant = async () => {
    try {
      setIsLoadingAssistant(true)
      setError(null)
      const { getAssistant } = await import("@/lib/api")
      const data = await getAssistant(assistantId!)
      
      if (data) {
        setAssistant(data)
        setName(data.name)
        setSelectedDataSources(data.dataSources)
      } else {
        setError("Assistant not found")
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load assistant")
      console.error("Error fetching assistant:", err)
    } finally {
      setIsLoadingAssistant(false)
    }
  }

  const fetchDataSources = async () => {
    try {
      setIsLoadingDataSources(true)
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
    
    if (!name || selectedDataSources.length === 0 || !assistantId) {
      return
    }

    try {
      setIsSubmitting(true)
      setError(null)

      const { updateAssistant } = await import("@/lib/api")
      await updateAssistant(assistantId, {
        name,
        dataSources: selectedDataSources,
      })
      
      console.log("Assistant updated successfully")
      
      // Redirect to dashboard after update
      router.push("/")
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update assistant")
      console.error("Error updating assistant:", err)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async () => {
    if (!assistantId) return

    try {
      setIsDeleting(true)
      setError(null)

      const { deleteAssistant } = await import("@/lib/api")
      await deleteAssistant(assistantId)
      
      console.log("Assistant deleted successfully")
      
      // Redirect to dashboard after deletion
      router.push("/")
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete assistant")
      console.error("Error deleting assistant:", err)
      setShowDeleteDialog(false)
    } finally {
      setIsDeleting(false)
    }
  }

  if (!assistantId) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-foreground mb-2">Assistant ID Required</h1>
          <p className="text-muted-foreground">Please provide an assistant ID in the URL.</p>
        </div>
      </div>
    )
  }

  if (isLoadingAssistant) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="h-8 w-8 text-muted-foreground mx-auto mb-3 animate-spin" />
          <p className="text-sm text-muted-foreground">Loading assistant...</p>
        </div>
      </div>
    )
  }

  if (error && !assistant) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-foreground mb-2">Error</h1>
          <p className="text-muted-foreground mb-4">{error}</p>
          <Link href="/">
            <Button>Back to Dashboard</Button>
          </Link>
        </div>
      </div>
    )
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
            <div className="flex-1">
              <h1 className="text-2xl font-mono font-bold text-foreground">Edit Assistant</h1>
              <p className="text-sm text-muted-foreground mt-1">Update assistant configuration</p>
            </div>
            <Button
              variant="destructive"
              size="sm"
              className="gap-2"
              onClick={() => setShowDeleteDialog(true)}
              disabled={isDeleting || isSubmitting}
            >
              <Trash2 className="h-4 w-4" />
              Delete
            </Button>
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
                {error && !isLoadingDataSources && dataSources.length === 0 && (
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
                {!isLoadingDataSources && dataSources.length > 0 && (
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
                                {dataSource.collectionName}
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
              {error && !isLoadingDataSources && dataSources.length > 0 && (
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
              {isSubmitting ? "Updating..." : "Update Assistant"}
            </Button>
          </div>
        </form>
      </main>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Assistant</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete "{assistant?.name}"? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeleting}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting && <Loader2 className="h-4 w-4 animate-spin mr-2" />}
              {isDeleting ? "Deleting..." : "Delete"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

export default function EditAssistantPage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen bg-background flex items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      }
    >
      <EditPageContent />
    </Suspense>
  )
}
