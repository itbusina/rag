"use client"

import type React from "react"

import { useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ArrowLeft, Upload, X, Loader2 } from "lucide-react"

export default function CreateDataSourcePage() {
  const router = useRouter()
  const [name, setName] = useState("")
  const [files, setFiles] = useState<File[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [uploadProgress, setUploadProgress] = useState<string>("")

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setFiles(Array.from(e.target.files))
    }
  }

  const removeFile = (index: number) => {
    setFiles(files.filter((_, i) => i !== index))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!name.trim()) {
      setError("Please enter a name for the data source")
      return
    }
    
    if (files.length === 0) {
      setError("Please select at least one file")
      return
    }

    try {
      setIsSubmitting(true)
      setError(null)
      setUploadProgress("Uploading files...")

      const { createDataSources } = await import("@/lib/api")
      const result = await createDataSources(files, name)
      console.log("Data sources created:", result)
      
      setUploadProgress("Processing complete!")
      
      // Redirect to data sources page after creation
      setTimeout(() => {
        router.push("/data-sources")
      }, 500)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create data source")
      console.error("Error creating data source:", err)
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
              <h1 className="text-2xl font-mono font-bold text-foreground">Create Data Source</h1>
              <p className="text-sm text-muted-foreground mt-1">Upload documents to create a new data source</p>
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
                placeholder="e.g., Product Documentation"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                disabled={isSubmitting}
                className="bg-background border-border text-foreground"
              />
              <p className="text-xs text-muted-foreground">Choose a descriptive name for your data source</p>
            </div>

            {/* File Upload */}
            <div className="space-y-2">
              <Label htmlFor="files" className="text-sm font-medium text-foreground">
                Documents
              </Label>
              <p className="text-xs text-muted-foreground mb-2">
                Upload PDF, TXT, MD, or other text documents. Each file will be processed and indexed.
              </p>
              <div className="border-2 border-dashed border-border bg-background p-8 text-center hover:border-primary/50 transition-colors">
                <Upload className="h-8 w-8 text-muted-foreground mx-auto mb-3" />
                <p className="text-sm text-muted-foreground mb-2">Drag and drop files here, or click to browse</p>
                <Input 
                  id="files" 
                  type="file" 
                  multiple 
                  onChange={handleFileChange} 
                  disabled={isSubmitting}
                  className="hidden"
                  accept=".pdf,.txt,.md,.doc,.docx" 
                />
                <Label htmlFor="files">
                  <Button 
                    type="button" 
                    variant="outline" 
                    size="sm" 
                    className="cursor-pointer bg-transparent" 
                    disabled={isSubmitting}
                    asChild
                  >
                    <span>Choose Files</span>
                  </Button>
                </Label>
              </div>

              {/* Selected Files */}
              {files.length > 0 && (
                <div className="mt-4 space-y-2">
                  <p className="text-sm font-medium text-foreground">{files.length} file(s) selected</p>
                  <div className="space-y-2 max-h-[300px] overflow-y-auto">
                    {files.map((file, index) => (
                      <div key={index} className="flex items-center justify-between p-3 bg-muted border border-border">
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-mono text-foreground truncate">{file.name}</p>
                          <p className="text-xs text-muted-foreground">{(file.size / 1024).toFixed(2)} KB</p>
                        </div>
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => removeFile(index)}
                          disabled={isSubmitting}
                          className="ml-2 h-8 w-8 p-0 hover:bg-destructive/10 hover:text-destructive"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    ))}
                  </div>
                </div>
              )}
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
                disabled={!name.trim() || files.length === 0 || isSubmitting}
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
