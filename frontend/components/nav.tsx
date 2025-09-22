"use client"

import { useSession, signIn, signOut } from "next-auth/react"

export default function Navbar() {
  const { data: session } = useSession()

  return (
    <nav className="flex items-center justify-between px-6 py-3 bg-gray-900 text-white">
      <h1 className="text-lg font-bold">Veiling Website</h1>

      <div>
        {session?.user ? (
          <div className="flex items-center gap-4">
            <span>{session.user.email}</span>

            <span
              className={`text-lg font-bold ${
                session.user.role === "user"
                  ? "text-green-500"
                  : session.user.role === "admin"
                  ? "text-yellow-500"
                  : "text-red-500"
              }`}
            >
              {
                session.user.role === "user"
                ? "O"
                : session.user.role === "admin"
                ? "A"
                : "X"
              }
            </span>

            <button
              onClick={() => signOut()}
              className="px-3 py-1 bg-red-500 rounded-lg text-sm"
            >
              Sign out
            </button>
          </div>
        ) : (
          <button
            onClick={() => signIn()}
            className="px-3 py-1 bg-blue-500 rounded-lg text-sm"
          >
            Sign in
          </button>
        )}
      </div>
    </nav>
  )
}
