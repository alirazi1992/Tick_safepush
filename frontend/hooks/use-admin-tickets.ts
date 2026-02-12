import { useCallback, useEffect, useState } from "react"
import { apiRequest } from "@/lib/api-client"
import type { ApiTicketCalendarResponse, ApiTicketStatus } from "@/lib/api-types"
import { useAuth } from "@/lib/auth-context"

export type AdminTicketsFilters = {
  status?: ApiTicketStatus
  start?: string
  end?: string
}

export function useAdminTickets(filters: AdminTicketsFilters = {}) {
  const { token } = useAuth()
  const [tickets, setTickets] = useState<ApiTicketCalendarResponse[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchTickets = useCallback(async () => {
    if (!token) return
    if (!filters.start || !filters.end) return

    setIsLoading(true)
    setError(null)
    try {
      const query = new URLSearchParams({
        start: filters.start,
        end: filters.end,
      })
      if (filters.status) {
        query.set("status", filters.status)
      }

      const data = await apiRequest<ApiTicketCalendarResponse[]>(
        `/api/tickets/calendar?${query.toString()}`,
        {
          method: "GET",
          token,
        }
      )
      setTickets(data)
    } catch (err: any) {
      setError(err?.message || "خطا در دریافت تیکت‌ها")
      setTickets([])
    } finally {
      setIsLoading(false)
    }
  }, [token, filters.start, filters.end, filters.status])

  useEffect(() => {
    fetchTickets()
  }, [fetchTickets])

  return {
    tickets,
    isLoading,
    error,
    refreshTickets: fetchTickets,
  }
}
































