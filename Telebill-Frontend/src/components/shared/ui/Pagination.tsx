// Pagination component — shows previous button, page numbers with ellipsis, and next button. Matches shadcn pagination style.

export type PaginationProps = {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
}

function getPageNumbers(
  currentPage: number,
  totalPages: number,
): (number | '...')[] {
  if (totalPages <= 0) return []
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, i) => i + 1)
  }
  if (currentPage <= 4) {
    return [1, 2, 3, 4, 5, '...', totalPages]
  }
  if (currentPage >= totalPages - 3) {
    return [
      1,
      '...',
      totalPages - 4,
      totalPages - 3,
      totalPages - 2,
      totalPages - 1,
      totalPages,
    ]
  }
  return [1, '...', currentPage - 1, currentPage, currentPage + 1, '...', totalPages]
}

export function Pagination({
  currentPage,
  totalPages = 10,
  onPageChange,
}: PaginationProps) {
  const pages = getPageNumbers(currentPage, totalPages)

  const prevDisabled = currentPage <= 1 || totalPages <= 0
  const nextDisabled = totalPages <= 0 || currentPage >= totalPages

  const btnBase =
    'rounded-md border border-gray-300 px-3 py-2 text-sm font-medium transition-colors'
  const btnEnabled = 'bg-white text-gray-700 hover:bg-gray-50'
  const btnDisabled = 'cursor-not-allowed bg-gray-50 text-gray-300'

  return (
    <div className="flex items-center justify-center gap-1 py-4">
      <button
        type="button"
        className={`${btnBase} ${prevDisabled ? btnDisabled : btnEnabled}`}
        disabled={prevDisabled}
        onClick={() => onPageChange(currentPage - 1)}
      >
        ← Previous
      </button>

      {pages.map((item, index) =>
        item === '...' ? (
          <span key={`ellipsis-${index}`} className="px-3 py-2 text-sm text-gray-400">
            ...
          </span>
        ) : (
          <button
            key={item}
            type="button"
            className={`h-9 w-9 rounded-md border text-sm font-medium transition-colors ${
              item === currentPage
                ? 'border-blue-600 bg-blue-600 text-white'
                : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
            }`}
            onClick={() => onPageChange(item)}
          >
            {item}
          </button>
        ),
      )}

      <button
        type="button"
        className={`${btnBase} ${nextDisabled ? btnDisabled : btnEnabled}`}
        disabled={nextDisabled}
        onClick={() => onPageChange(currentPage + 1)}
      >
        Next →
      </button>
    </div>
  )
}
