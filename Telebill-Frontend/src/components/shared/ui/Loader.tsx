// Loader spinner component — centered animated ring. Use fullscreen prop to cover the entire viewport.

export type LoaderProps = {
  fullscreen?: boolean
}

export function Loader({ fullscreen = false }: LoaderProps) {
  const outerClass = fullscreen
    ? 'fixed inset-0 z-50 flex items-center justify-center bg-white/80 backdrop-blur-sm'
    : 'flex w-full items-center justify-center p-12'

  return (
    <div className={outerClass}>
      <div className="h-10 w-10 animate-spin rounded-full border-4 border-blue-600 border-t-transparent" />
    </div>
  )
}
