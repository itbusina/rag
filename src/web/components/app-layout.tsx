"use client"

import type React from "react"
import { useState } from "react"
import { SidebarNav } from "./sidebar-nav"
import { cn } from "@/lib/utils"

export function AppLayout({ children }: { children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false)

  return (
    <div className="flex min-h-screen">
      <SidebarNav collapsed={collapsed} setCollapsed={setCollapsed} />
      <main className={cn("flex-1 transition-all duration-300", collapsed ? "ml-16" : "ml-64")}>{children}</main>
    </div>
  )
}
