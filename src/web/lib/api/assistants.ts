/**
 * Assistants API Client
 * Centralized API calls for assistant-related operations
 */

import { API_BASE_URL } from "../config"

// Types
export interface Assistant {
  id: string
  name: string
  dataSources: string[]
}

export interface CreateAssistantRequest {
  name: string
  dataSources: string[]
}

export interface CreateAssistantResponse {
  id: string
  name: string
}

export interface QueryAssistantResponse {
  response: string
}

// API Client
export class AssistantsApiClient {
  private baseUrl: string

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl
  }

  /**
   * Fetch all assistants
   */
  async getAll(): Promise<Assistant[]> {
    const response = await fetch(`${this.baseUrl}/assistants`)
    
    if (!response.ok) {
      throw new Error(`Failed to fetch assistants: ${response.statusText}`)
    }
    
    return response.json()
  }

  /**
   * Get a single assistant by ID
   */
  async getById(id: string): Promise<Assistant | null> {
    const assistants = await this.getAll()
    return assistants.find((a) => a.id === id) || null
  }

  /**
   * Create a new assistant
   */
  async create(data: CreateAssistantRequest): Promise<CreateAssistantResponse> {
    const response = await fetch(`${this.baseUrl}/assistants`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    })

    if (!response.ok) {
      throw new Error(`Failed to create assistant: ${response.statusText}`)
    }

    return response.json()
  }

  /**
   * Query an assistant with a question
   */
  async query(id: string, question: string): Promise<QueryAssistantResponse> {
    const response = await fetch(`${this.baseUrl}/assistants/${id}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(question),
    })

    if (!response.ok) {
      throw new Error(`Failed to query assistant: ${response.statusText}`)
    }

    return response.json()
  }

  /**
   * Delete an assistant (if endpoint exists)
   */
  async delete(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/assistants/${id}`, {
      method: "DELETE",
    })

    if (!response.ok) {
      throw new Error(`Failed to delete assistant: ${response.statusText}`)
    }
  }

  /**
   * Update an assistant (if endpoint exists)
   */
  async update(id: string, data: Partial<CreateAssistantRequest>): Promise<void> {
    const response = await fetch(`${this.baseUrl}/assistants/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    })

    if (!response.ok) {
      throw new Error(`Failed to update assistant: ${response.statusText}`)
    }
  }
}

// Export a singleton instance
export const assistantsApi = new AssistantsApiClient()

// Export convenience functions for direct use
export const getAssistants = () => assistantsApi.getAll()
export const getAssistant = (id: string) => assistantsApi.getById(id)
export const createAssistant = (data: CreateAssistantRequest) => assistantsApi.create(data)
export const queryAssistant = (id: string, question: string) => assistantsApi.query(id, question)
export const deleteAssistant = (id: string) => assistantsApi.delete(id)
export const updateAssistant = (id: string, data: Partial<CreateAssistantRequest>) => 
  assistantsApi.update(id, data)

