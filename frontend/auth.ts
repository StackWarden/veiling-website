import NextAuth from "next-auth"
import Resend from "next-auth/providers/resend"
import { PrismaAdapter } from "@auth/prisma-adapter"
import { PrismaClient } from "@prisma/client"

const prisma = new PrismaClient()
 
export const { handlers, auth, signIn, signOut } = NextAuth({
    adapter: PrismaAdapter(prisma),
    providers: [
      Resend({
        apiKey: process.env.AUTH_RESEND_KEY!,
        from: "no-reply@mail.joeribrinks.nl"
      }),
    ],
    callbacks: {
    async session({ session, user }) {
      if (session.user) {
        session.user.role = user.role
      }
      return session
    },
  },
})
