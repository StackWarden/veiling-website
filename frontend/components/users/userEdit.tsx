"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";

type UserDto = {
  id: string;
  username: string;
  role: string;
};

const ROLE_OPTIONS = [
  { label: "Auctioneer", value: "auctioneer" },
  { label: "Supplier", value: "supplier" },
  { label: "Buyer", value: "buyer" },
  { label: "Admin", value: "admin" },
];

export default function UserEdit() {
  const { id } = useParams();
  const router = useRouter();

  const [user, setUser] = useState<UserDto | null>(null);
  const [role, setRole] = useState("buyer");

  useEffect(() => {
    const fetchUser = async () => {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/users/${id}`,
        { credentials: "include" }
      );

      if (!res.ok) {
        setUser(null);
        return;
      }

      const data = (await res.json()) as UserDto;
      setUser(data);
      setRole((data.role || "buyer").toLowerCase());
    };

    fetchUser();
  }, [id]);

  const saveChanges = async () => {
    if (!user) return;

    const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/users/${id}`, {
      method: "PUT",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ role }),
    });

    if (!res.ok) return;

    alert("User updated!");
    router.push("/users");
  };

  if (!user) {
    return <p className="text-center mt-10">Loading...</p>;
  }

  return (
    <div className="w-full flex flex-col items-center mt-12">
      <h1 className="text-[32px] font-bold text-[#162218] mb-10">
        User info
      </h1>

      <div className="flex gap-16">
        <div className="border border-[#D9D9D9] rounded-xl p-8 w-[380px] shadow-sm">
          <h2 className="text-2xl font-semibold text-[#162218] mb-8">
            {user.username}
          </h2>

          <div className="flex justify-between items-center mb-5">
            <span className="font-medium">Username</span>
            <span className="text-gray-700">{user.username}</span>
          </div>

          <div className="flex justify-between items-center mb-5">
            <span className="font-medium">Role</span>

            <select
              value={role}
              onChange={(e) => setRole(e.target.value)}
              className="border border-gray-300 rounded px-3 py-1 text-sm w-40"
            >
              {ROLE_OPTIONS.map((r) => (
                <option key={r.value} value={r.value}>
                  {r.label}
                </option>
              ))}
            </select>
          </div>

          <button
            onClick={saveChanges}
            className="mt-8 w-full bg-[#162218] text-white py-3 rounded-lg hover:bg-[#0f1c14] transition"
          >
            Save changes
          </button>
        </div>
      </div>
    </div>
  );
}
