"use client"

import { Suspense } from "react"
import { useSearchParams } from "next/navigation"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { ArrowLeft, Loader2 } from "lucide-react"

function EditPageContent() {
  const searchParams = useSearchParams()
  const assistantId = searchParams.get("id")

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
              <h1 className="text-2xl font-mono font-bold text-foreground">Edit Assistant</h1>
              <p className="text-sm text-muted-foreground mt-1">Update assistant configuration</p>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-6 py-8 max-w-3xl">
        <Card className="p-12 border border-border bg-card text-center">
          <h3 className="text-lg font-semibold text-foreground mb-2">Edit Functionality Coming Soon</h3>
          <p className="text-muted-foreground mb-6">
            Assistant editing will be implemented when the backend API supports it.
          </p>
          <p className="text-xs text-muted-foreground">
            Assistant ID: {assistantId}
          </p>
        </Card>
      </main>
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
