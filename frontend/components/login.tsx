"use client";

import { useSearchParams } from "next/navigation";
import { useState } from "react";

export default function Login() {
  const searchParams = useSearchParams();
  const initialMessage = searchParams.get("message") || "";

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [info, setInfo] = useState(initialMessage);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError("");
    setInfo("");

    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/jwt`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      const token = await res.text();

      if (!res.ok) {
        throw new Error(token || "Login failed");
      }

      if (!token || token.trim().length < 20) {
        throw new Error("Invalid token received from server.");
      }

      localStorage.setItem("jwt", token);

      window.location.href = "/";
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unknown error occurred");
    }
  };

  return (
    <>
      <h1 className="text-2xl font-semibold text-center mb-6">Login</h1>

      {info && <p className="text-green-600 text-center mb-2">{info}</p>}
      {error && <p className="text-red-600 text-center mb-2">{error}</p>}

      <form onSubmit={handleSubmit} className="flex flex-col space-y-4">
        <div className="flex flex-col">
          <label htmlFor="email" className="text-gray-700 mb-1">Email</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Enter your email"
            className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div className="flex flex-col">
          <label htmlFor="password" className="text-gray-700 mb-1">Password</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter your password"
            className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <button
          type="submit"
          className="mt-4 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors"
        >
          Login
        </button>
      </form>
    </>
  );
}