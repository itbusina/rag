/**
 * Data Sources API Client
 * Centralized API calls for data source operations
 */

import { API_BASE_URL } from "../config"

// Types
export enum DataSourceType {
  Stream = 0,
  File = 1,
  GitHub = 2,
  Url = 3,
  Sitemap = 4,
  Confluence = 5,
}

export interface DataSource {
  id: string
  name: string
  dataSourceType: DataSourceType
  dataSourceValue: string
  createdDate: string
  collectionName: string
}

// API Client
export class DataSourcesApiClient {
  private baseUrl: string

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl
  }

  /**
   * Fetch all data sources
   */
  async getAll(): Promise<DataSource[]> {
    const response = await fetch(`${this.baseUrl}/datasources`)
    
    if (!response.ok) {
      throw new Error(`Failed to fetch data sources: ${response.statusText}`)
    }
    
    return response.json()
  }

  /**
   * Get a single data source by ID
   */
  async getById(id: string): Promise<DataSource | null> {
    const dataSources = await this.getAll()
    return dataSources.find((ds) => ds.id === id) || null
  }

  /**
   * Upload files to create data sources
   */
  async create(files: File[], name: string): Promise<string[]> {
    const formData = new FormData()
    files.forEach((file) => {
      formData.append("files", file)
    })
    formData.append("name", name)

    const response = await fetch(`${this.baseUrl}/datasources?type=file`, {
      method: "POST",
      body: formData,
    })

    if (!response.ok) {
      throw new Error(`Failed to create data sources: ${response.statusText}`)
    }

    return response.json()
  }

  /**
   * Create a Confluence data source
   */
  async createConfluence(params: {
    name: string
    serverUrl: string
    personalToken: string
    parentUrl: string
  }): Promise<string> {
    const formData = new FormData()
    formData.append("name", params.name)
    formData.append("url", params.serverUrl)
    formData.append("token", params.personalToken)
    formData.append("parentPageId", params.parentUrl)

    const response = await fetch(`${this.baseUrl}/datasources?type=confluence`, {
      method: "POST",
      body: formData,
    })

    if (!response.ok) {
      throw new Error(`Failed to create Confluence data source: ${response.statusText}`)
    }

    return response.json()
  }

  /**
   * Create a GitHub data source
   */
  async createGitHub(params: {
    name: string
    repositoryUrl: string
    accessToken?: string
  }): Promise<string> {
    const formData = new FormData()
    formData.append("name", params.name)
    formData.append("url", params.repositoryUrl)
    if (params.accessToken) {
      formData.append("token", params.accessToken)
    }

    const response = await fetch(`${this.baseUrl}/datasources?type=github`, {
      method: "POST",
      body: formData,
    })

    if (!response.ok) {
      throw new Error(`Failed to create GitHub data source: ${response.statusText}`)
    }

    return response.json()
  }

  /**
   * Delete a data source
   */
  async delete(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/datasources/${id}`, {
      method: "DELETE",
    })

    if (!response.ok) {
      throw new Error(`Failed to delete data source: ${response.statusText}`)
    }
  }
}

// Export a singleton instance
export const dataSourcesApi = new DataSourcesApiClient()

// Export convenience functions
export const getDataSources = () => dataSourcesApi.getAll()
export const getDataSource = (id: string) => dataSourcesApi.getById(id)
export const createDataSources = (files: File[], name: string) => dataSourcesApi.create(files, name)
export const createConfluenceDataSource = (params: {
  name: string
  serverUrl: string
  personalToken: string
  parentUrl: string
}) => dataSourcesApi.createConfluence(params)
export const createGitHubDataSource = (params: {
  name: string
  repositoryUrl: string
  accessToken?: string
}) => dataSourcesApi.createGitHub(params)
export const deleteDataSource = (id: string) => dataSourcesApi.delete(id)

// Helper function to get data source type label
export const getDataSourceTypeLabel = (type: DataSourceType): string => {
  switch (type) {
    case DataSourceType.Stream:
      return "Stream"
    case DataSourceType.File:
      return "File"
    case DataSourceType.GitHub:
      return "GitHub"
    case DataSourceType.Url:
      return "URL"
    case DataSourceType.Sitemap:
      return "Sitemap"
    case DataSourceType.Confluence:
      return "Confluence"
    default:
      return "Unknown"
  }
}

