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

export default function CreateGitHubDataSourcePage() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [repositoryUrl, setRepositoryUrl] = useState("")
  const [accessToken, setAccessToken] = useState("")
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [uploadProgress, setUploadProgress] = useState<string>("")

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!name.trim()) {
      setError("Please enter a name for the data source")
      return
    }
    
    if (!repositoryUrl.trim()) {
      setError("Please enter the GitHub repository URL")
      return
    }

    try {
      setIsSubmitting(true)
      setError(null)
      setUploadProgress("Creating GitHub data source...")

      const { createGitHubDataSource } = await import("@/lib/api")
      const result = await createGitHubDataSource({
        name,
        repositoryUrl,
        accessToken: accessToken.trim() || undefined
      })
      console.log("GitHub data source created:", result)
      
      setUploadProgress("Processing complete!")
      
      // Redirect to data sources page after creation
      setTimeout(() => {
        router.push("/data-sources")
      }, 500)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create GitHub data source")
      console.error("Error creating GitHub data source:", err)
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
              <h1 className="text-2xl font-mono font-bold text-foreground">Create GitHub Data Source</h1>
              <p className="text-sm text-muted-foreground mt-1">Connect to a GitHub repository to index commit messages</p>
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
                placeholder="e.g., My Project Repository"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">Choose a descriptive name for your GitHub data source</p>
            </div>

            {/* GitHub Repository URL */}
            <div className="space-y-2">
              <Label htmlFor="repositoryUrl" className="text-sm font-medium text-foreground">
                GitHub Repository URL
              </Label>
              <Input
                id="repositoryUrl"
                type="url"
                placeholder="e.g., https://github.com/owner/repository"
                value={repositoryUrl}
                onChange={(e) => setRepositoryUrl(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">The URL of the GitHub repository (supports both public and private repos)</p>
            </div>

            {/* GitHub Access Token (Optional) */}
            <div className="space-y-2">
              <Label htmlFor="accessToken" className="text-sm font-medium text-foreground">
                GitHub Access Token <span className="text-muted-foreground font-normal">(Optional)</span>
              </Label>
              <Input
                id="accessToken"
                type="password"
                placeholder="Enter your GitHub personal access token"
                value={accessToken}
                onChange={(e) => setAccessToken(e.target.value)}
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">
                Required for private repositories and higher rate limits. Create a token from GitHub Settings → Developer settings → Personal access tokens
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
                disabled={!name.trim() || !repositoryUrl.trim() || isSubmitting}
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

