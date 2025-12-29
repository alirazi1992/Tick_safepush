/**
 * Ticket Status Definitions and Persian Labels
 * 
 * This file serves as the single source of truth for ticket status types and their Persian labels.
 * Backend stores statuses as English enum keys, but frontend displays Persian labels.
 */

export type TicketStatus = "Submitted" | "Viewed" | "Open" | "InProgress" | "Resolved" | "Closed"

export const TICKET_STATUS_LABELS: Record<TicketStatus, string> = {
  Submitted: "ثبت شد",
  Viewed: "مشاهده شد",
  Open: "باز",
  InProgress: "در حال انجام",
  Resolved: "حل شده",
  Closed: "بسته",
}

export const TICKET_STATUS_OPTIONS: Array<{ value: TicketStatus; label: string }> = [
  { value: "Submitted", label: TICKET_STATUS_LABELS.Submitted },
  { value: "Viewed", label: TICKET_STATUS_LABELS.Viewed },
  { value: "Open", label: TICKET_STATUS_LABELS.Open },
  { value: "InProgress", label: TICKET_STATUS_LABELS.InProgress },
  { value: "Resolved", label: TICKET_STATUS_LABELS.Resolved },
  { value: "Closed", label: TICKET_STATUS_LABELS.Closed },
]

/**
 * Get Persian label for a ticket status
 */
export function getTicketStatusLabel(status: TicketStatus): string {
  return TICKET_STATUS_LABELS[status] || status
}








