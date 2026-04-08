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

interface RegisterRequest {
  username: string
  email: string
  password: string
  confirmPassword: string
  position: string
  firstName?: string
  lastName?: string
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
    register: builder.mutation<void, RegisterRequest>({
      query: (body) => ({
        url: '/auth/register',
        method: 'POST',
        body,
      }),
    }),
  }),
})

export const { useLoginMutation, useRegisterMutation } = authApi
