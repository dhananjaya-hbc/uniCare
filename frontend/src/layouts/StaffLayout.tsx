import { NavLink, Outlet } from 'react-router-dom'
import { ThemeToggle } from '@/components/common/ThemeToggle'

// TODO(auth): show only the sections the signed-in role may use, via useAuth().hasRole.
const links = [
  { to: '/students', label: 'Students' },
  { to: '/system-status', label: 'System status' },
]

export function StaffLayout() {
  return (
    <div className="flex min-h-svh bg-background">
      <aside className="flex w-56 shrink-0 flex-col border-r border-border p-4">
        <div className="mb-6 flex items-center justify-between gap-2">
          <p className="text-sm font-semibold">UniCare</p>
        </div>
        <nav className="flex flex-col gap-1">
          {links.map(({ to, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `rounded-md px-3 py-2 text-sm transition-colors ${
                  isActive
                    ? 'bg-accent font-medium text-accent-foreground'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground'
                }`
              }
            >
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="mt-auto border-t border-border pt-4">
          <ThemeToggle />
        </div>
      </aside>
      <main className="min-w-0 flex-1">
        <Outlet />
      </main>
    </div>
  )
}
