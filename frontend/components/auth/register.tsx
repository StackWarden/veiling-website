"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { usePost } from "../api/post";

type RegisterRole = string;

export default function Register() {
  const router = useRouter();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");

  const [role, setRole] = useState<string>("");
  const [roles, setRoles] = useState<RegisterRole[]>([]);
  const [rolesLoading, setRolesLoading] = useState(false);

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");

  // Fetch roles from backend
  useEffect(() => {
    async function loadRoles() {
      setRolesLoading(true);
      setError("");

      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/auth/registerRoles`,
          {
            method: "GET",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
          }
        );

        if (!res.ok) {
          const text = await res.text().catch(() => "");
          throw new Error(text || `Failed to load roles: ${res.statusText}`);
        }

        const data = (await res.json()) as RegisterRole[];

        const clean = Array.isArray(data) ? data : [];
        setRoles(clean);

        // default select first role if none selected yet
        if (!role && clean.length > 0) {
          setRole(clean[0]);
        }
      } catch (e) {
        const err = e instanceof Error ? e : new Error("Unknown error");
        setError(err.message);
      } finally {
        setRolesLoading(false);
      }
    }

    loadRoles();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const { execute: register } = usePost<{
    name: string;
    email: string;
    password: string;
    role: string;
  }>({
    route: "/auth/register",
    onSuccess: () => {
      router.push(
        `/login?message=${encodeURIComponent(
          "Account created successfully! You can now log in."
        )}`
      );
    },
    onError: (err) => {
      setError(err.message);
    },
  });

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError("");

    if (!role) {
      setError("Please select a role.");
      return;
    }

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    await register({
      name,
      email,
      password,
      role,
    });
  };

  return (
    <div className="min-h-screen bg-white flex items-center justify-center p-[3px]">
      <div className="relative flex w-full max-w-[97%] max-h-[97vh] bg-[#0f1c14] rounded-[20px] overflow-hidden shadow-2xl border border-white">
        {/* Left image */}
        <div className="relative w-[50%] hidden md:block p-[2px]">
          <div className="rounded-[18px] overflow-hidden h-full">
            <Image
              src="/leaves.png"
              alt="Green leaves"
              width={800}
              height={800}
              className="object-cover w-full h-full"
            />
          </div>
        </div>

        {/* Right form */}
        <div className="w-[50%] flex items-center justify-center bg-[#0f1c14] py-6">
          <div className="bg-white rounded-[20px] shadow-2xl p-8 w-full max-w-[26rem] mx-6">
            {/* Logo */}
            <div className="flex justify-center mb-5">
              <div className="bg-[#0f1c14] rounded-full p-1 flex items-center">
                <Image
                  src="/logo.png"
                  alt="logo"
                  width={40}
                  height={40}
                  className="w-14 h-14"
                />
              </div>
            </div>

            <h1 className="text-2xl font-semibold text-center mb-4 text-[#0f1c14]">
              Register
            </h1>

            {error && <p className="text-red-600 text-center mb-2">{error}</p>}

            <form onSubmit={handleSubmit} className="flex flex-col space-y-2">
              {/* Name */}
              <div className="flex flex-col">
                <label htmlFor="name" className="text-gray-800 mb-1 font-bold">
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

              {/* Email */}
              <div className="flex flex-col">
                <label htmlFor="email" className="text-gray-800 mb-1 font-bold">
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

              {/* Role */}
              <div className="flex flex-col">
                <label htmlFor="role" className="text-gray-800 mb-1 font-bold">
                  Role
                </label>
                <select
                  id="role"
                  value={role}
                  onChange={(e) => setRole(e.target.value)}
                  className="border border-gray-300 rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-green-600"
                  disabled={rolesLoading}
                  required
                >
                  {rolesLoading ? (
                    <option value="">Loading roles…</option>
                  ) : roles.length === 0 ? (
                    <option value="">No roles available</option>
                  ) : (
                    roles.map((r) => (
                      <option key={r} value={r}>
                        {r}
                      </option>
                    ))
                  )}
                </select>
              </div>

              {/* Password */}
              <div className="flex flex-col">
                <label htmlFor="password" className="text-gray-800 mb-1 font-bold">
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

              {/* Confirm */}
              <div className="flex flex-col">
                <label
                  htmlFor="confirmPassword"
                  className="text-gray-800 mb-1 font-bold"
                >
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

              {/* Login link */}
              <div className="flex items-center justify-between text-sm">
                <p>
                  Already have an account?{" "}
                  <a
                    href="/login"
                    className="text-yellow-500 hover:text-yellow-600 font-medium"
                  >
                    Log in!
                  </a>
                </p>
              </div>

              {/* Submit */}
              <button
                type="submit"
                className="mt-3 bg-[#0f1c14] text-white py-2 rounded-lg hover:bg-green-900 transition-colors"
                disabled={rolesLoading}
              >
                Register
              </button>
            </form>
          </div>
        </div>

        {/* Help button */}
        <div className="absolute bottom-6 right-6">
          <button className="bg-white rounded-full p-3 shadow-md hover:bg-gray-100">
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
      </div>
    </div>
  );
}
