"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Plus, FileText, Trash2, Loader2 } from "lucide-react"
import { ThemeToggle } from "@/components/theme-toggle"
import { 
  getDataSources, 
  deleteDataSource, 
  getDataSourceTypeLabel,
  type DataSource 
} from "@/lib/api"

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  })
}

export default function DataSourcesPage() {
  const [dataSources, setDataSources] = useState<DataSource[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchDataSources()
  }, [])

  const fetchDataSources = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const data = await getDataSources()
      setDataSources(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : "An error occurred")
      console.error("Error fetching data sources:", err)
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this data source? This action cannot be undone.")) {
      return
    }

    try {
      await deleteDataSource(id)
      // Refresh the list after deletion
      await fetchDataSources()
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to delete data source")
      console.error("Error deleting data source:", err)
    }
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b border-border bg-card">
        <div className="container mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-mono font-bold text-foreground">Data Sources</h1>
              <p className="text-sm text-muted-foreground mt-1">Manage your document collections</p>
            </div>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              <Link href="/data-sources/create">
                <Button className="gap-2">
                  <Plus className="h-4 w-4" />
                  Create Data Source
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
            <p className="text-muted-foreground">Loading data sources...</p>
          </Card>
        )}

        {/* Error State */}
        {error && !isLoading && (
          <Card className="p-12 border border-destructive bg-card text-center">
            <p className="text-destructive mb-4">{error}</p>
            <Button onClick={fetchDataSources} variant="outline">
              Try Again
            </Button>
          </Card>
        )}

        {/* Empty State */}
        {!isLoading && !error && dataSources.length === 0 && (
          <Card className="p-12 border border-border bg-card text-center">
            <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-foreground mb-2">No data sources yet</h3>
            <p className="text-muted-foreground mb-6">Create your first data source to get started</p>
            <Link href="/data-sources/create">
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Create Data Source
              </Button>
            </Link>
          </Card>
        )}

        {/* Data Sources List */}
        {!isLoading && !error && dataSources.length > 0 && (
          <div className="grid grid-cols-1 gap-4">
            {dataSources.map((dataSource) => (
              <Card
                key={dataSource.id}
                className="p-6 border border-border bg-card hover:border-primary/50 transition-colors"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <h3 className="text-lg font-mono font-semibold text-foreground">
                        {dataSource.collectionName}
                      </h3>
                      <span className="text-xs font-mono px-2 py-1 bg-primary/10 text-primary border border-primary/20">
                        {getDataSourceTypeLabel(dataSource.dataSourceType)}
                      </span>
                    </div>
                    <p className="text-sm text-muted-foreground mb-4 leading-relaxed break-all">
                      {dataSource.dataSourceValue}
                    </p>
                    <div className="flex items-center gap-6 text-xs text-muted-foreground font-mono">
                      <div>
                        <span>Created {formatDate(dataSource.createdDate)}</span>
                      </div>
                      <div className="text-xs text-muted-foreground/70">
                        <span>ID: {dataSource.id}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 ml-4">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDelete(dataSource.id)}
                      className="gap-2 bg-transparent hover:border-destructive hover:text-destructive"
                    >
                      <Trash2 className="h-4 w-4" />
                      Delete
                    </Button>
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
