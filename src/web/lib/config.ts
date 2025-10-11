// API Configuration
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5067"

export const API_ENDPOINTS = {
  assistants: `${API_BASE_URL}/assistants`,
  dataSources: `${API_BASE_URL}/datasources`,
} as const

