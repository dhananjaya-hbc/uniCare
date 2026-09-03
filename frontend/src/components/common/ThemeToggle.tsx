import { Moon, Sun } from 'lucide-react'
import { useTheme } from 'next-themes'
import { Button } from '@/components/ui/button'

/**
 * Switches between light and dark. The initial theme follows the operating
 * system; this only overrides it, and next-themes persists the choice.
 */
export function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  return (
    <Button
      variant="outline"
      size="sm"
      aria-label={isDark ? 'Switch to light theme' : 'Switch to dark theme'}
      onClick={() => setTheme(isDark ? 'light' : 'dark')}
    >
      {isDark ? <Moon className="size-4" /> : <Sun className="size-4" />}
      {isDark ? 'Dark' : 'Light'}
    </Button>
  )
}
