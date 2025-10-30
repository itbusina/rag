"use client"

import type React from "react"

import { useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ArrowLeft, Loader2 } from "lucide-react"

export default function CreateSitemapDataSourcePage() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [sitemapUrl, setSitemapUrl] = useState("")
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [uploadProgress, setUploadProgress] = useState<string>("")

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!name.trim()) {
      setError("Please enter a name for the data source")
      return
    }
    
    if (!sitemapUrl.trim()) {
      setError("Please enter the sitemap URL")
      return
    }

    try {
      setIsSubmitting(true)
      setError(null)
      setUploadProgress("Creating Sitemap data source...")

      const { createSitemapDataSource } = await import("@/lib/api")
      const result = await createSitemapDataSource({
        name,
        sitemapUrl
      })
      console.log("Sitemap data source created:", result)
      
      setUploadProgress("Processing complete!")
      
      // Redirect to data sources page after creation
      setTimeout(() => {
        router.push("/data-sources")
      }, 500)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create Sitemap data source")
      console.error("Error creating Sitemap data source:", err)
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
            <Link href="/data-sources">
              <Button variant="ghost" size="sm" className="gap-2" disabled={isSubmitting}>
                <ArrowLeft className="h-4 w-4" />
                Back
              </Button>
            </Link>
            <div>
              <h1 className="text-2xl font-mono font-bold text-foreground">Create Sitemap Data Source</h1>
              <p className="text-sm text-muted-foreground mt-1">Index web pages from a sitemap.xml file</p>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-6 py-8 max-w-3xl">
        <Card className="p-8 border border-border bg-card">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Name Input */}
            <div className="space-y-2">
              <Label htmlFor="name" className="text-sm font-medium text-foreground">
                Data Source Name
              </Label>
              <Input
                id="name"
                type="text"
                placeholder="e.g., Company Website"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">Choose a descriptive name for your Sitemap data source</p>
            </div>

            {/* Sitemap URL */}
            <div className="space-y-2">
              <Label htmlFor="sitemapUrl" className="text-sm font-medium text-foreground">
                Sitemap URL
              </Label>
              <Input
                id="sitemapUrl"
                type="url"
                placeholder="e.g., https://example.com/sitemap.xml"
                value={sitemapUrl}
                onChange={(e) => setSitemapUrl(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">
                The URL of the sitemap.xml file. The sitemap will be parsed and all URLs will be indexed.
              </p>
            </div>

            {/* Upload Progress */}
            {isSubmitting && uploadProgress && (
              <div className="p-4 border border-primary bg-primary/10 text-primary text-sm flex items-center gap-2">
                <Loader2 className="h-4 w-4 animate-spin" />
                {uploadProgress}
              </div>
            )}

            {/* Error Message */}
            {error && (
              <div className="p-4 border border-destructive bg-destructive/10 text-destructive text-sm">
                {error}
              </div>
            )}

            {/* Actions */}
            <div className="flex items-center gap-3 pt-4">
              <Button 
                type="submit" 
                className="flex-1 gap-2" 
                disabled={!name.trim() || !sitemapUrl.trim() || isSubmitting}
              >
                {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />}
                {isSubmitting ? "Creating..." : "Create Data Source"}
              </Button>
              <Link href="/data-sources" className="flex-1">
                <Button 
                  type="button" 
                  variant="outline" 
                  className="w-full bg-transparent"
                  disabled={isSubmitting}
                >
                  Cancel
                </Button>
              </Link>
            </div>
          </form>
        </Card>
      </main>
    </div>
  )
}

