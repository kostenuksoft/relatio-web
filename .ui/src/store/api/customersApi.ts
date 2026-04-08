import { baseApi } from './baseApi'
import type { CustomerDto, PagedResult } from '../../types'

interface GetCustomersParams {
  page?: number
  pageSize?: number
  name?: string
}

interface CreateCustomerRequest {
  name: string
  email: string
  phone?: string
  industry?: string
}

export const customersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getCustomers: builder.query<PagedResult<CustomerDto>, GetCustomersParams>({
      query: ({ page = 1, pageSize = 20, name } = {}) => ({
        url: '/customers',
        params: { page, pageSize, ...(name ? { name } : {}) },
      }),
      providesTags: ['Customer'],
    }),
    createCustomer: builder.mutation<CustomerDto, CreateCustomerRequest>({
      query: (body) => ({
        url: '/customers',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Customer'],
    }),
    deleteCustomer: builder.mutation<void, string>({
      query: (id) => ({ url: `/customers/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Customer'],
    }),
  }),
})

export const { useGetCustomersQuery, useCreateCustomerMutation, useDeleteCustomerMutation } = customersApi
