"use client";

import { useEffect, useState } from "react";

type AuthState = {
  role: string | null;
  status: "loading" | "authenticated" | "unauthenticated";
};

export default function useAuth(): AuthState {
  const [state, setState] = useState<AuthState>({
    role: null,
    status: "loading",
  });

  useEffect(() => {
    const fetchInfo = async () => {
      try {
        const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/info`, {
          credentials: "include",
        });
        if (!res.ok) {
          setState({ role: null, status: "unauthenticated" });
          return;
        }
        const data = await res.json();
        setState({
          role: data.role?.toLowerCase() ?? null,
          status: "authenticated",
        });
      } catch {
        setState({ role: null, status: "unauthenticated" });
      }
    };

    fetchInfo();
  }, []);

  return state;
}
