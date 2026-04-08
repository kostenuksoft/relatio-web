import { baseApi } from './baseApi'
import type { ContactDto, PagedResult } from '../../types'

interface GetContactsParams {
  page?: number
  pageSize?: number
}

interface CreateContactRequest {
  firstName: string
  lastName: string
  email?: string
  phone?: string
  position?: string
  customerId: string
}

export const contactsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getContacts: builder.query<PagedResult<ContactDto>, GetContactsParams>({
      query: ({ page = 1, pageSize = 20 } = {}) => ({
        url: '/contacts',
        params: { page, pageSize },
      }),
      providesTags: ['Contact'],
    }),
    createContact: builder.mutation<ContactDto, CreateContactRequest>({
      query: (body) => ({
        url: '/contacts',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Contact'],
    }),
  }),
})

export const { useGetContactsQuery, useCreateContactMutation } = contactsApi
