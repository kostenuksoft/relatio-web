import { type FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLoginMutation, useRegisterMutation } from '../store/api/authApi'
import { setCredentials } from '../store/slices/authSlice'
import { useAppDispatch } from '../store/hooks'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Field } from '@/components/ui/field'
import { Button } from '@/components/ui/button'
import { parseApiError } from '@/lib/errors'

type Mode = 'signin' | 'register'

function EyeIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
      <circle cx="12" cy="12" r="3" />
    </svg>
  )
}

function EyeOffIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
      <line x1="1" y1="1" x2="23" y2="23" />
    </svg>
  )
}

function Logo() {
  return (
    <div className="flex items-center gap-2.5">
      <svg width="28" height="28" viewBox="0 0 28 28" fill="none">
        <rect width="28" height="28" fill="white" />
        <path d="M7 7h7l4.5 7L14 21H7l4.5-7L7 7z" fill="black" fillOpacity="0.85" />
        <path d="M14 7h7l-2.5 7 2.5 7h-7l4.5-7L14 7z" fill="black" fillOpacity="0.3" />
      </svg>
      <span className="font-code font-semibold text-sm tracking-tight text-foreground">relatio</span>
    </div>
  )
}

const fieldInput = 'w-full bg-transparent text-sm text-foreground py-1 focus:outline-none placeholder:text-white/20'

export function LoginPage() {
  const [mode, setMode] = useState<Mode>('signin')

  const [credential, setCredential] = useState('')
  const [password, setPassword] = useState('')
  const [showPwd, setShowPwd] = useState(false)
  const [login, { isLoading: isSigningIn, error: signInError }] = useLoginMutation()

  const [username, setUsername] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [position, setPosition] = useState('')
  const [regEmail, setRegEmail] = useState('')
  const [regPassword, setRegPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showRegPwd, setShowRegPwd] = useState(false)
  const [showConfirmPwd, setShowConfirmPwd] = useState(false)
  const [pwdMismatch, setPwdMismatch] = useState(false)
  const [registered, setRegistered] = useState(false)
  const [register, { isLoading: isRegistering, error: registerError }] = useRegisterMutation()

  const dispatch = useAppDispatch()
  const navigate = useNavigate()

  const { general: signInGeneral, fields: signInFields } = parseApiError(signInError)
  const { general: regGeneral, fields: regFields } = parseApiError(registerError)

  async function handleSignIn(e: FormEvent) {
    e.preventDefault()
    try {
      const data = await login({ credential, password }).unwrap()
      dispatch(setCredentials({ token: data.accessToken, refreshToken: data.refreshToken }))
      navigate('/', { replace: true })
    } catch { /* shown via signInError */ }
  }

  async function handleRegister(e: FormEvent) {
    e.preventDefault()
    if (regPassword !== confirmPassword) { setPwdMismatch(true); return }
    setPwdMismatch(false)
    try {
      await register({
        username, email: regEmail, password: regPassword, confirmPassword, position,
        ...(firstName ? { firstName } : {}),
        ...(lastName ? { lastName } : {}),
      }).unwrap()
      setRegistered(true)
    } catch { /* shown via registerError */ }
  }

  function switchMode(m: Mode) {
    setMode(m)
    setRegistered(false)
    setPwdMismatch(false)
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <Card>
          <CardHeader className="pb-0 pt-5 px-5">
            <div className="mb-4">
              <Logo />
            </div>
            <div className="flex gap-6 border-b border-white/8">
              {(['signin', 'register'] as Mode[]).map((m) => (
                <button
                  key={m}
                  type="button"
                  onClick={() => switchMode(m)}
                  className={`pb-3 text-sm font-medium transition-colors cursor-pointer ${
                    mode === m
                      ? 'border-b-2 border-white text-foreground -mb-px'
                      : 'text-muted-foreground hover:text-foreground'
                  }`}
                >
                  {m === 'signin' ? 'Sign In' : 'Sign Up'}
                </button>
              ))}
            </div>
          </CardHeader>

          <CardContent className="pt-5 px-5 pb-5">
            {mode === 'signin' && (
              <form onSubmit={handleSignIn} className="space-y-5">
                <Field label="Email or username" error={signInFields['credential']}>
                  <input
                    type="text"
                    value={credential}
                    onChange={(e) => setCredential(e.target.value)}
                    required
                    autoFocus
                    placeholder="admin@relatio.co"
                    className={fieldInput}
                  />
                </Field>

                <Field label="Password" error={signInFields['password']}>
                  <div className="relative flex items-center">
                    <input
                      type={showPwd ? 'text' : 'password'}
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      required
                      placeholder="••••••••"
                      className={fieldInput + ' pr-7'}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPwd((v) => !v)}
                      className="absolute right-0 text-white/30 hover:text-white/60 transition-colors cursor-pointer"
                    >
                      {showPwd ? <EyeOffIcon /> : <EyeIcon />}
                    </button>
                  </div>
                </Field>

                {signInGeneral && <p className="text-xs text-destructive">{signInGeneral}</p>}

                <Button type="submit" disabled={isSigningIn} size="lg" className="w-full mt-2">
                  {isSigningIn ? 'Signing in…' : 'Sign in'}
                </Button>

                <button
                  type="button"
                  onClick={() => {
                    dispatch(setCredentials({ token: 'dev-bypass', refreshToken: '' }))
                    navigate('/', { replace: true })
                  }}
                  className="w-full text-xs text-white/20 hover:text-white/40 transition-colors cursor-pointer pt-1"
                >
                  skip (dev)
                </button>
              </form>
            )}

            {mode === 'register' && !registered && (
              <form onSubmit={handleRegister} className="space-y-5">
                <Field label="Username" error={regFields['username']}>
                  <input type="text" value={username} onChange={(e) => setUsername(e.target.value)} required placeholder="john.doe" className={fieldInput} />
                </Field>

                <div className="grid grid-cols-2 gap-3">
                  <Field label="First name">
                    <input type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="John" className={fieldInput} />
                  </Field>
                  <Field label="Last name">
                    <input type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Doe" className={fieldInput} />
                  </Field>
                </div>

                <Field label="Position" error={regFields['position']}>
                  <input type="text" value={position} onChange={(e) => setPosition(e.target.value)} placeholder="Sales Manager" className={fieldInput} />
                </Field>

                <Field label="Email" error={regFields['email']}>
                  <input type="email" value={regEmail} onChange={(e) => setRegEmail(e.target.value)} required placeholder="you@company.com" className={fieldInput} />
                </Field>

                <Field label="Password" error={regFields['password']}>
                  <div className="relative flex items-center">
                    <input
                      type={showRegPwd ? 'text' : 'password'}
                      value={regPassword}
                      onChange={(e) => setRegPassword(e.target.value)}
                      required
                      placeholder="••••••••"
                      className={fieldInput + ' pr-7'}
                    />
                    <button type="button" onClick={() => setShowRegPwd((v) => !v)} className="absolute right-0 text-white/30 hover:text-white/60 transition-colors cursor-pointer">
                      {showRegPwd ? <EyeOffIcon /> : <EyeIcon />}
                    </button>
                  </div>
                </Field>

                <Field label="Confirm password" error={pwdMismatch ? 'Passwords do not match' : regFields['confirmpassword']}>
                  <div className="relative flex items-center">
                    <input
                      type={showConfirmPwd ? 'text' : 'password'}
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      required
                      placeholder="••••••••"
                      className={fieldInput + ' pr-7'}
                    />
                    <button type="button" onClick={() => setShowConfirmPwd((v) => !v)} className="absolute right-0 text-white/30 hover:text-white/60 transition-colors cursor-pointer">
                      {showConfirmPwd ? <EyeOffIcon /> : <EyeIcon />}
                    </button>
                  </div>
                </Field>

                {regGeneral && <p className="text-xs text-destructive">{regGeneral}</p>}

                <Button type="submit" disabled={isRegistering} size="lg" className="w-full mt-2">
                  {isRegistering ? 'Submitting…' : 'Request access'}
                </Button>
              </form>
            )}

            {mode === 'register' && registered && (
              <div className="py-8 text-center space-y-2">
                <p className="text-sm font-medium text-foreground">Access request submitted.</p>
                <p className="text-xs text-muted-foreground leading-relaxed">
                  An admin will review your request and activate your account.
                </p>
                <button
                  type="button"
                  onClick={() => switchMode('signin')}
                  className="text-xs text-white/40 hover:text-white/70 transition-colors cursor-pointer mt-4 block mx-auto"
                >
                  Back to sign in →
                </button>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
