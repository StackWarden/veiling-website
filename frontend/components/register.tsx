"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

export default function Register() {
  const router = useRouter();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError("");

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, email, password }),
      });

      const text = await res.text();
      const data = (() => {
        try { return JSON.parse(text); } catch { return { message: text }; }
      })();

      if (!res.ok) throw new Error(data.message || "Registration failed");

      router.push(`/login?message=${encodeURIComponent("Account created successfully! You can now log in.")}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unknown error occurred");
    }
  };

  return (
    <div className="min-h-screen bg-[#0f1c14] flex items-center justify-between">

      <div className="w-1/2 h-screen hidden md:block">
        <img
          src="/leaves.png"
          alt="Green leaves"
          className="object-cover w-full h-full"
        />
      </div>

      <div className="w-full md:w-1/2 flex items-center justify-center bg-[#0f1c14] h-screen">
        <div className="bg-white rounded-2xl shadow-2xl p-8 w-full max-w-sm mx-12">
          
          <div className="flex justify-center mb-6">
            <div className="bg-[#0f1c14] rounded-full p-6 flex items-center justify-center">
              <img 
              src="/logo.png" 
              alt="logo" 
              className="w-10 h-10" />
            </div>
        </div>

        <h1 className="text-2xl font-semibold text-center mb-6 text-[#0f1c14]">
            Register
        </h1>

        {error && <p className="text-red-600 text-center">{error}</p>}

        <form onSubmit={handleSubmit} className="flex flex-col space-y-4">
          <div className="flex flex-col">
            <label htmlFor="name" className="text-gray-700 mb-1 font-medium">
              Name
            </label>
            <input
              type="text"
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Enter your name"
              className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-green-600"
              required
            />
          </div>

          <div className="flex flex-col">
            <label htmlFor="email" className="text-gray-700 mb-1 font-medium">
              Email
            </label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Enter your email"
              className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-green-600"
              required
            />
          </div>

          <div className="flex flex-col">
            <label htmlFor="password" className="text-gray-700 mb-1 font-medium">
              Password
            </label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-green-600"
              required
            />
          </div>

          <div className="flex flex-col">
            <label htmlFor="confirmPassword" className="text-gray-700 mb-1 font-medium">
              Confirm Password
            </label>
            <input
              type="password"
              id="confirmPassword"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Re-enter your password"
              className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-green-600"
              required
            />
          </div>

          <label className="flex items-center space-x-2 text-sm">
              <input type="checkbox" className="accent-green-600" />
              <span>Keep me logged in</span>
          </label>

          <button
            type="submit"
            className="mt-4 bg-[#0f1c14] text-white py-2 rounded-lg hover:bg-green-900 transition-colors"
          >
           Register
          </button>
      </form>
    </div>
  </div>

  
  <button className="fixed bottom-4 right-4 bg-white rounded-full p-3 shadow-md hover:bg-gray-100">
    <svg
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
      strokeWidth={1.5}
      stroke="currentColor"
      className="w-5 h-5 text-gray-800"
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M8.625 9.75a3.375 3.375 0 116.75 0c0 1.59-.832 2.13-1.678 2.727-.763.537-1.322 1.1-1.322 2.023v.75m0 2.25h.008v.008H12v-.008z"
      />
    </svg>
  </button>
</div>
);
}

  