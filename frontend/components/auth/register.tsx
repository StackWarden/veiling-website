"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { usePost } from "../api/post";

export default function Register() {
  const router = useRouter();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");

  const {
    loading,
    error: postError,
    execute: register,
  } = usePost<{
    name: string;
    email: string;
    password: string;
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

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    await register({
      name,
      email,
      password,
    });
  };

  return (
    <div className="min-h-screen bg-white flex items-center justify-center p-[3px]">
      {/* Groene hoofdcontainer met witte rand */}
      <div className="relative flex w-full max-w-[97%] max-h-[97vh] bg-[#0f1c14] rounded-[20px] overflow-hidden shadow-2xl border border-white">
        {/* Linkerkant met image*/}
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


        {/* recherkant met register form */}
        <div className="w-[50%] flex items-center justify-center bg-[#0f1c14] py-6">
          {/* witte box met form */}
          <div className="bg-white rounded-[20px] shadow-2xl p-8 w-full max-w-[26rem] mx-6">
            {/*logo boven */}
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
            {/* titel */}
            <h1 className="text-2xl font-semibold text-center mb-4 text-[#0f1c14]">
              Register
            </h1>

            {error && <p className="text-red-600 text-center">{error}</p>}

            <form onSubmit={handleSubmit} className="flex flex-col space-y-2">
              {/* naamveld */}
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
              {/* emailveld */}
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
              {/* passwordveld */}
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
              {/* confirm password veld */}
              <div className="flex flex-col">
                <label htmlFor="confirmPassword" className="text-gray-800 mb-1 font-bold">
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
              {/* keep me logged in checkbox */}
              <div className="flex items-center justify-start text-sm">
                <label className="flex items-center space-x-2 text-sm">
                  <input type="checkbox" className="accent-green-600" />
                  <span>Keep me logged in</span>
                </label>
              </div>
              {/* register button */}
              <button
                type="submit"
                className="mt-3 bg-[#0f1c14] text-white py-2 rounded-lg hover:bg-green-900 transition-colors"
              >
                Register
              </button>
            </form>
          </div>
        </div>
        {/* help button rechtsonder */}
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

