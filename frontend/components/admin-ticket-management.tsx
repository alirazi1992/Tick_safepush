"use client"

import { AdminManagementTickets } from "@/components/admin-management-tickets"
import type { Ticket } from "@/types"

interface AdminTicketManagementProps {
  authToken?: string | null
  tickets?: Ticket[]
}

export function AdminTicketManagement({
  authToken,
  tickets,
}: AdminTicketManagementProps) {
  return (
    <div className="space-y-6">
      <AdminManagementTickets authToken={authToken} tickets={tickets} />
    </div>
  )
}

