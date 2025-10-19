// API Configuration
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || ""

export const API_ENDPOINTS = {
  assistants: `${API_BASE_URL}/assistants`,
  dataSources: `${API_BASE_URL}/datasources`,
} as const

