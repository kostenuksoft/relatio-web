import { baseApi } from './baseApi'
import type { DealDto, PagedResult } from '../../types'

interface GetDealsParams {
  page?: number
  pageSize?: number
}

interface CreateDealRequest {
  title: string
  amount: number
  currency: string
  customerId: string
  expectedCloseDate?: string
  notes?: string
}

export const dealsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getDeals: builder.query<PagedResult<DealDto>, GetDealsParams>({
      query: ({ page = 1, pageSize = 20 } = {}) => ({
        url: '/deals',
        params: { page, pageSize },
      }),
      providesTags: ['Deal'],
    }),
    createDeal: builder.mutation<DealDto, CreateDealRequest>({
      query: (body) => ({
        url: '/deals',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Deal'],
    }),
  }),
})

export const { useGetDealsQuery, useCreateDealMutation } = dealsApi
