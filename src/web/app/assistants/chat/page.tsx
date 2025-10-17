"use client"

import type React from "react"

import { useState, useRef, useEffect, Suspense } from "react"
import { useSearchParams } from "next/navigation"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { ArrowLeft, Send, Bot, User, Loader2 } from "lucide-react"
import { getAssistant, queryAssistant } from "@/lib/api"
import { MarkdownContent } from "@/components/markdown-content"

type Message = {
  id: string
  role: "user" | "assistant"
  content: string
  timestamp: Date
}

function ChatPageContent() {
  const searchParams = useSearchParams()
  const assistantId = searchParams.get("id")
  
  const [messages, setMessages] = useState<Message[]>([
    {
      id: "1",
      role: "assistant",
      content:
        "Hello! I'm your assistant. I can help you find information from the connected data sources. What would you like to know?",
      timestamp: new Date(),
    },
  ])
  const [input, setInput] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [assistantName, setAssistantName] = useState<string>("")
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  useEffect(() => {
    if (assistantId) {
      fetchAssistantDetails()
    }
  }, [assistantId])

  const fetchAssistantDetails = async () => {
    if (!assistantId) return
    
    try {
      const assistant = await getAssistant(assistantId)
      if (assistant) {
        setAssistantName(assistant.name)
      }
    } catch (err) {
      console.error("Error fetching assistant details:", err)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim() || isLoading || !assistantId) return

    const userMessage: Message = {
      id: Date.now().toString(),
      role: "user",
      content: input,
      timestamp: new Date(),
    }

    const currentInput = input
    setMessages([...messages, userMessage])
    setInput("")
    setIsLoading(true)

    try {
      const data = await queryAssistant(assistantId, currentInput)
      
      const assistantMessage: Message = {
        id: (Date.now() + 1).toString(),
        role: "assistant",
        content: data.response || "I couldn't find an answer to your question.",
        timestamp: new Date(),
      }
      setMessages((prev) => [...prev, assistantMessage])
    } catch (err) {
      console.error("Error querying assistant:", err)
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        role: "assistant",
        content: "Sorry, I encountered an error while processing your question. Please try again.",
        timestamp: new Date(),
      }
      setMessages((prev) => [...prev, errorMessage])
    } finally {
      setIsLoading(false)
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

  return (
    <div className="min-h-screen bg-background flex flex-col">
      {/* Header */}
      <header className="border-b border-border bg-card flex-shrink-0">
        <div className="container mx-auto px-6 py-4">
          <div className="flex items-center gap-4">
            <Link href="/">
              <Button variant="ghost" size="sm" className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Back
              </Button>
            </Link>
            <div>
              <h1 className="text-2xl font-mono font-bold text-foreground">Chat</h1>
              <p className="text-sm text-muted-foreground mt-1">{assistantName || "Loading..."}</p>
            </div>
          </div>
        </div>
      </header>

      {/* Chat Messages */}
      <main className="flex-1 overflow-y-auto">
        <div className="container mx-auto px-6 py-8 max-w-4xl">
          <div className="space-y-6">
            {messages.map((message) => (
              <div
                key={message.id}
                className={`flex gap-4 ${message.role === "user" ? "justify-end" : "justify-start"}`}
              >
                {message.role === "assistant" && (
                  <div className="flex-shrink-0 w-8 h-8 bg-primary text-primary-foreground flex items-center justify-center">
                    <Bot className="h-4 w-4" />
                  </div>
                )}
                <Card
                  className={`p-4 max-w-2xl border ${
                    message.role === "user"
                      ? "bg-primary text-primary-foreground border-primary"
                      : "bg-card border-border"
                  }`}
                >
                  {message.role === "assistant" ? (
                    <div className="text-sm">
                      <MarkdownContent content={message.content} />
                    </div>
                  ) : (
                    <p className="text-sm leading-relaxed">{message.content}</p>
                  )}
                  <p
                    className={`text-xs mt-2 font-mono ${
                      message.role === "user" ? "text-primary-foreground/70" : "text-muted-foreground"
                    }`}
                  >
                    {message.timestamp.toLocaleTimeString()}
                  </p>
                </Card>
                {message.role === "user" && (
                  <div className="flex-shrink-0 w-8 h-8 bg-accent text-accent-foreground flex items-center justify-center">
                    <User className="h-4 w-4" />
                  </div>
                )}
              </div>
            ))}
            {isLoading && (
              <div className="flex gap-4 justify-start">
                <div className="flex-shrink-0 w-8 h-8 bg-primary text-primary-foreground flex items-center justify-center">
                  <Bot className="h-4 w-4" />
                </div>
                <Card className="p-4 max-w-2xl border bg-card border-border">
                  <p className="text-sm text-muted-foreground">Thinking...</p>
                </Card>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>
        </div>
      </main>

      {/* Input Area */}
      <div className="border-t border-border bg-card flex-shrink-0">
        <div className="container mx-auto px-6 py-4 max-w-4xl">
          <form onSubmit={handleSubmit} className="flex gap-3">
            <Input
              type="text"
              placeholder="Ask a question about your documents..."
              value={input}
              onChange={(e) => setInput(e.target.value)}
              disabled={isLoading}
              className="flex-1 bg-background border-border text-foreground"
            />
            <Button type="submit" disabled={!input.trim() || isLoading} className="gap-2">
              <Send className="h-4 w-4" />
              Send
            </Button>
          </form>
        </div>
      </div>
    </div>
  )
}

export default function ChatPage() {
  return (
    <Suspense
      fallback={
        <div className="min-h-screen bg-background flex items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      }
    >
      <ChatPageContent />
    </Suspense>
  )
}
