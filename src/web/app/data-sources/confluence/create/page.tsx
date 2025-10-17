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

export default function CreateConfluenceDataSourcePage() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [serverUrl, setServerUrl] = useState("")
  const [personalToken, setPersonalToken] = useState("")
  const [parentUrl, setParentUrl] = useState("")
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [uploadProgress, setUploadProgress] = useState<string>("")

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!name.trim()) {
      setError("Please enter a name for the data source")
      return
    }
    
    if (!serverUrl.trim()) {
      setError("Please enter the Confluence server URL")
      return
    }

    if (!personalToken.trim()) {
      setError("Please enter your personal access token")
      return
    }

    if (!parentUrl.trim()) {
      setError("Please enter the parent URL")
      return
    }

    try {
      setIsSubmitting(true)
      setError(null)
      setUploadProgress("Creating Confluence data source...")

      const { createConfluenceDataSource } = await import("@/lib/api")
      const result = await createConfluenceDataSource({
        name,
        serverUrl,
        personalToken,
        parentUrl
      })
      console.log("Confluence data source created:", result)
      
      setUploadProgress("Processing complete!")
      
      // Redirect to data sources page after creation
      setTimeout(() => {
        router.push("/data-sources")
      }, 500)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create Confluence data source")
      console.error("Error creating Confluence data source:", err)
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
              <h1 className="text-2xl font-mono font-bold text-foreground">Create Confluence Data Source</h1>
              <p className="text-sm text-muted-foreground mt-1">Connect to Atlassian Confluence to index documentation</p>
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
                placeholder="e.g., Engineering Wiki"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">Choose a descriptive name for your Confluence data source</p>
            </div>

            {/* Confluence Server URL */}
            <div className="space-y-2">
              <Label htmlFor="serverUrl" className="text-sm font-medium text-foreground">
                Confluence Server URL
              </Label>
              <Input
                id="serverUrl"
                type="url"
                placeholder="e.g., https://your-domain.atlassian.net"
                value={serverUrl}
                onChange={(e) => setServerUrl(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">The base URL of your Confluence instance</p>
            </div>

            {/* Personal Access Token */}
            <div className="space-y-2">
              <Label htmlFor="personalToken" className="text-sm font-medium text-foreground">
                Personal Access Token
              </Label>
              <Input
                id="personalToken"
                type="password"
                placeholder="Enter your Confluence API token"
                value={personalToken}
                onChange={(e) => setPersonalToken(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">
                Create a token from your Confluence account settings
              </p>
            </div>

            {/* Parent URL */}
            <div className="space-y-2">
              <Label htmlFor="parentUrl" className="text-sm font-medium text-foreground">
                Parent URL
              </Label>
              <Input
                id="parentUrl"
                type="url"
                placeholder="e.g., https://your-domain.atlassian.net/wiki/spaces/SPACE"
                value={parentUrl}
                onChange={(e) => setParentUrl(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">
                The parent page or space URL to start indexing from
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
                disabled={!name.trim() || !serverUrl.trim() || !personalToken.trim() || !parentUrl.trim() || isSubmitting}
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

