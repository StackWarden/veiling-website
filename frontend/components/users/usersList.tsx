"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import useGet from "../api/get";
import { RoleGate } from "../RoleGate";
import List, { ListHeader } from "../list";
import useAuth from "../../hooks/useAuth";

/* ---------- Types ---------- */

type User = {
  id: string;
  name: string;
  email: string;
  createdAt: string;
};

/* ---------- Component ---------- */

export default function UsersList() {
  const [users, setUsers] = useState<User[]>([]);
  const [deleting, setDeleting] = useState(false);
  const { role } = useAuth();

  const { loading, execute } = useGet<User>({
    route: "/users",
    autoFetch: false,
    onSuccess: (data) => {
      const formatted = (Array.isArray(data) ? data : []).map((u) => ({
        ...u,
        createdAt: u.createdAt
          ? new Date(u.createdAt).toLocaleString()
          : "-",
      }));
      setUsers(formatted);
    },
  });

  useEffect(() => {
    execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure?")) return;
    
    setDeleting(true);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/users/delete/${id}`,
        { method: "DELETE", credentials: "include" }
      );

      if (!res.ok) return;

      setUsers((prev) => prev.filter((u) => u.id !== id));
    } finally {
      setDeleting(false);
    }
  };

  // Build headers array conditionally
  const headers: ListHeader[] = [
    { key: "name", label: "Name", align: "start" },
    { key: "email", label: "Email", align: "start" },
    { key: "createdAt", label: "Created At", align: "start" },
  ];

  // Conditionally add Actions header for admins
  if (role === "admin") {
    headers.push({ key: "actions", label: "Actions", align: "end" });
  }

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        {/* Header */}
        <div className="flex items-center w-full pt-8 pb-1">
          <div className="flex-1" />

          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Users
          </h1>

          <div className="flex-1 flex justify-end" />
        </div>

        {/* Content */}
        {(loading || deleting) && (
          <p className="text-gray-500 text-center py-6">Loading users...</p>
        )}

        {!loading && !deleting && users.length === 0 && (
          <p className="text-gray-500 text-center py-6">No users available.</p>
        )}

        {!loading && !deleting && users.length > 0 && (
          <List
            headers={headers}
            rows={users.map((user) => {
              const row: Record<string, unknown> = {
                ...user,
              };
              if (role === "admin") {
                row.actions = (
                  <RoleGate allow={["admin"]}>
                    <div className="flex gap-6 justify-end">
                      <Link
                        href={`/users/edit/${user.id}`}
                        className="hover:underline"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDelete(user.id);
                        }}
                        className="hover:cursor-pointer hover:underline underline-offset-2 text-red-600 hover:text-red-400"
                        type="button"
                      >
                        Delete
                      </button>
                    </div>
                  </RoleGate>
                );
              }
              return row;
            })}
            rowKey="id"
            onRowClick={(user) => {
              if (role === "admin") {
                window.location.href = `/users/edit/${user.id}`;
              }
            }}
          />
        )}
      </div>
    </section>
  );
}
