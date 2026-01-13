"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import useGet from "../api/get";

type UserRow = {
  id: string;
  name: string;
  email: string;
  createdAt: string;
};

export default function UsersList() {
  const [users, setUsers] = useState<UserRow[]>([]);

  const { loading, execute } = useGet<UserRow>({
    route: "/users",
    autoFetch: false,
    onSuccess: (data) => setUsers(data),
  });

  useEffect(() => {
    execute();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure?")) return;

    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/users/delete/${id}`,
      { method: "DELETE", credentials: "include" }
    );

    if (!res.ok) return;

    setUsers((prev) => prev.filter((u) => u.id !== id));
  };

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        {/* Header */}
        <div className="flex items-center mb-6 w-full pt-8 pb-4">
          <div className="flex-1" />

          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Users
          </h1>

          <div className="flex-1" />
        </div>

        {/* Content */}
        {loading ? (
          <p className="text-gray-500 text-center py-6">Loading users...</p>
        ) : users.length === 0 ? (
          <p className="text-gray-500 text-center py-6">No users available.</p>
        ) : (
          <div className="overflow-hidden rounded-xl border border-[#D9D9D9] p-4">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="text-[#4D4D4D]">
                  <th className="p-3 text-start">Username</th>
                  <th className="p-3 text-center">Email</th>
                  <th className="p-3 text-end">Created At</th>
                  <th className="p-3 text-end">Actions</th>
                </tr>
              </thead>

              <tbody className="text-[#1A1A1A]">
                {users.map((u) => (
                  <tr
                    key={u.id}
                    className="hover:bg-[#162218] hover:text-white transition cursor-pointer"
                    onClick={() => (window.location.href = `/users/edit/${u.id}`)}
                  >
                    <td className="p-4 rounded-l-2xl">{u.name}</td>
                    <td className="p-4 text-center">{u.email}</td>
                    <td className="p-4 text-end">
                      {u.createdAt ? new Date(u.createdAt).toLocaleString() : "-"}
                    </td>

                    <td className="p-4 text-end rounded-r-2xl">
                      <div className="flex gap-6 justify-end">
                        <Link
                          href={`/users/edit/${u.id}`}
                          onClick={(e) => e.stopPropagation()}
                          className="hover:underline"
                        >
                          Edit
                        </Link>

                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDelete(u.id);
                          }}
                          className="hover:underline text-red-600 hover:text-red-400"
                          type="button"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}
