"use client"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { Button } from "@/components/ui/button"
import { ChevronLeft, Database, Bot } from "lucide-react"
import { cn } from "@/lib/utils"

const menuItems = [
  {
    title: "Assistants",
    href: "/",
    icon: Bot,
  },
  {
    title: "Data Sources",
    href: "/data-sources",
    icon: Database,
  },
]

export function SidebarNav({
  collapsed,
  setCollapsed,
}: {
  collapsed: boolean
  setCollapsed: (collapsed: boolean) => void
}) {
  const pathname = usePathname()

  return (
    <aside
      className={cn(
        "fixed left-0 top-0 h-screen bg-sidebar border-r border-sidebar-border transition-all duration-300 z-50",
        collapsed ? "w-16" : "w-64",
      )}
    >
      <div className="flex flex-col h-full">
        {/* Header */}
        <div className="p-4 border-b border-sidebar-border flex items-center justify-between">
          {!collapsed && (
            <div>
              <h2 className="font-mono font-bold text-sidebar-foreground">RAG Admin</h2>
              <p className="text-xs text-muted-foreground">v1.0.0</p>
            </div>
          )}
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setCollapsed(!collapsed)}
            className={cn("h-8 w-8 p-0 hover:bg-sidebar-accent", collapsed && "mx-auto")}
          >
            <ChevronLeft className={cn("h-4 w-4 transition-transform", collapsed && "rotate-180")} />
          </Button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-2">
          <ul className="space-y-1">
            {menuItems.map((item) => {
              const Icon = item.icon
              const isActive = pathname === item.href || (item.href !== "/" && pathname.startsWith(item.href))

              return (
                <li key={item.href}>
                  <Link href={item.href}>
                    <Button
                      variant="ghost"
                      className={cn(
                        "w-full justify-start gap-3 h-10 hover:bg-sidebar-accent",
                        collapsed && "justify-center px-0",
                        isActive && "bg-sidebar-accent text-sidebar-primary font-semibold",
                      )}
                    >
                      <Icon className="h-5 w-5 shrink-0" />
                      {!collapsed && <span>{item.title}</span>}
                    </Button>
                  </Link>
                </li>
              )
            })}
          </ul>
        </nav>
      </div>
    </aside>
  )
}
