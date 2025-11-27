"use client";

import { useState } from "react";

interface LogoutButtonProps {
  className?: string;
  redirectTo?: string;
}

export default function LogoutButton({
  className,
  redirectTo = "/login?message=Logged%20out",
}: LogoutButtonProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleLogout = async () => {
    setLoading(true);
    setError("");

    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/auth/logout`,
        {
          method: "POST",
          credentials: "include",
        }
      );

      if (!res.ok) {
        throw new Error("Logout failed");
      }

      // Force refresh to clear any client state
      window.location.href = redirectTo;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Unexpected logout error";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={className}>
      <button
        onClick={handleLogout}
        disabled={loading}
        className="bg-[#0f1c14] text-white px-4 py-2 rounded-lg hover:bg-green-900 transition disabled:opacity-50"
      >
        {loading ? "Logging out..." : "Logout"}
      </button>
    </div>
  );
}
