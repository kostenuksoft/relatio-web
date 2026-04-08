import { baseApi } from './baseApi'

interface LoginRequest {
  credential: string
  password: string
}

interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export const authApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (body) => ({
        url: '/auth/login',
        method: 'POST',
        body,
      }),
    }),
  }),
})

export const { useLoginMutation } = authApi
