"use client"

import { useSession } from "next-auth/react"

export default function Test() {
  const { data: session } = useSession()

    if (session?.user.role === null) {
        return (
            <div>
                <p>You don't have any role</p>
            </div>
        )
    } else if (session?.user.role === "user") {
        return (
            <div>
                <p>Your role is user</p>
            </div>
        )
    }
}
