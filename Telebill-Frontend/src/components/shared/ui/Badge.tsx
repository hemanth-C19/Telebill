export type BadgeProps = {
  status: string
}

const green = new Set([
  'Active',
  'Approved',
  'Accepted',
  'Paid',
  'Acked',
  'Finalized',
  'Resolved',
])
const blue = new Set(['Open', 'Ready', 'Loaded'])
const purple = new Set(['Submitted', 'Requested', 'ReadyForCoding'])
const red = new Set([
  'Denied',
  'Rejected',
  'Failed',
  'Expired',
  'Inactive',
  'WrittenOff',
])
const grayMuted = new Set(['Draft', 'Closed', 'Dismissed', 'Voided'])
const yellow = new Set([
  'PartiallyPaid',
  'Appealed',
  'Generated',
  'Batched',
])

function statusToColorClass(status: string): string {
  if (green.has(status)) return 'bg-green-100 text-green-800'
  if (blue.has(status)) return 'bg-blue-100 text-blue-800'
  if (purple.has(status)) return 'bg-purple-100 text-purple-800'
  if (red.has(status)) return 'bg-red-100 text-red-800'
  if (grayMuted.has(status)) return 'bg-gray-100 text-gray-800'
  if (yellow.has(status)) return 'bg-yellow-100 text-yellow-800'
  return 'bg-gray-100 text-gray-600'
}

export function Badge({ status }: BadgeProps) {
  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${statusToColorClass(status)}`}
    >
      {status}
    </span>
  )
}

export default Badge
