export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface CustomerDto {
  id: string
  name: string
  email: string
  phone?: string
  industry?: string
  status: string
  createdAt: string
}

export interface ContactDto {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email?: string
  phone?: string
  position?: string
  customerId: string
  createdAt: string
}

export interface DealDto {
  id: string
  title: string
  amount: number
  currency: string
  stage: string
  customerId: string
  expectedCloseDate?: string
  notes?: string
  createdAt: string
}
