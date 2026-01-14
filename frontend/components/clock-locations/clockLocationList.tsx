"use client";

import React, { useEffect, useState } from "react";
import useGet from "../api/get";
import useDelete from "../api/delete";
import { RoleGate } from "../RoleGate";
import List, { ListHeader } from "../list";
import useAuth from "../../hooks/useAuth";
import Image from "next/image";
import { usePost } from "../api/post";

/* ---------- Types ---------- */

type ClockLocation = {
  id: string;
  name: string;
  createdAt: string;
};

type CreateClockLocationDto = {
  name: string;
};

/* ---------- Component ---------- */

export default function ClockLocationList() {
  const [clockLocations, setClockLocations] = useState<ClockLocation[]>([]);
  const [newClockLocationName, setNewClockLocationName] = useState("");
  const [showCreateForm, setShowCreateForm] = useState(false);
  const { role } = useAuth();

  const { loading, execute: fetchClockLocations } = useGet<ClockLocation>({
    route: "/clock-locations",
    autoFetch: false,
    onSuccess: (data) => {
      const formatted = (Array.isArray(data) ? data : []).map((cl) => ({
        ...cl,
        createdAt: cl.createdAt
          ? new Date(cl.createdAt).toLocaleString()
          : "-",
      }));
      setClockLocations(formatted);
    },
  });

  const {
    loading: creating,
    error: createError,
    execute: createClockLocation,
  } = usePost<CreateClockLocationDto, ClockLocation>({
    route: "/clock-locations",
    onSuccess: () => {
      setNewClockLocationName("");
      setShowCreateForm(false);
      fetchClockLocations();
    },
  });

  const { loading: deleting, error: deleteError, execute: deleteClockLocation } = useDelete({
    baseRoute: "/clock-locations",
    onSuccess: () => {
      fetchClockLocations();
    },
  });

  useEffect(() => {
    fetchClockLocations();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    const trimmedName = newClockLocationName.trim();
    if (!trimmedName) {
      return;
    }
    
    await createClockLocation({ name: trimmedName });
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this clock location?")) return;
    await deleteClockLocation(id);
  };

  // Build headers array
  const headers: ListHeader[] = [
    { key: "name", label: "Name", align: "start" },
    { key: "createdAt", label: "Created At", align: "start" },
  ];

  // Conditionally add Actions header for admins
  if (role === "admin") {
    headers.push({ key: "actions", label: "Actions", align: "end" });
  }

  return (
    <RoleGate allow={["admin"]}>
      <section className="w-full flex flex-col items-center mt-12 px-4">
        <div className="w-full max-w-[90rem] px-4">
          {/* Header */}
          <div className="flex items-center w-full pt-8 pb-1">
            <div className="flex-1" />

            <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
              Clock Locations
            </h1>

            <RoleGate allow={["admin"]} fallback={<div className="flex-1 flex justify-end" />}>
              <div className="flex-1 flex justify-end">
                <button
                  onClick={() => setShowCreateForm(!showCreateForm)}
                  className="flex flex-row items-center gap-2 p-1 rounded-full hover:cursor-pointer"
                  type="button"
                >
                  <span className="text-[#162218] font-medium">
                    {showCreateForm ? "Cancel" : "Add Clock Location"}
                  </span>
                  <Image
                    src="/images/Plus.svg"
                    alt="Add Clock Location Icon"
                    width={40}
                    height={40}
                    priority
                  />
                </button>
              </div>
            </RoleGate>
          </div>

          {/* Create Form */}
          {showCreateForm && (
            <div className="mb-6 p-6 bg-gray-50 border rounded-lg">
              <h2 className="text-xl font-bold mb-4">Add New Clock Location</h2>
              <form
                onSubmit={handleCreate}
                className="flex gap-2"
              >
                <input
                  type="text"
                  value={newClockLocationName}
                  onChange={(e) => setNewClockLocationName(e.target.value)}
                  placeholder="Clock location name"
                  className="flex-1 border rounded-lg p-2"
                  disabled={creating}
                />
                <button
                  type="submit"
                  disabled={creating || !newClockLocationName.trim()}
                  className="bg-[#162218] text-white px-4 py-2 rounded-lg font-semibold hover:bg-[#0f1c14] disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {creating ? "Adding..." : "Add"}
                </button>
              </form>
              {createError && (
                <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded">
                  <p className="text-red-600 text-sm font-medium">Error: {createError}</p>
                </div>
              )}
            </div>
          )}

          {deleteError && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-600 text-sm font-medium">Error: {deleteError}</p>
            </div>
          )}

          {/* Content */}
          {loading || deleting ? (
            <p className="text-gray-500 text-center py-6">
              Loading clock locations...
            </p>
          ) : clockLocations.length === 0 ? (
            <p className="text-gray-500 text-center py-6">
              No clock locations available.
            </p>
          ) : (
            <List
              headers={headers}
              rows={clockLocations.map((cl) => {
                const row: Record<string, unknown> = {
                  id: cl.id,
                  name: cl.name,
                  createdAt: cl.createdAt,
                };
                if (role === "admin") {
                  row.actions = (
                    <RoleGate allow={["admin"]}>
                      <div className="flex gap-6 justify-end">
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDelete(cl.id);
                          }}
                          className="hover:underline text-red-600 hover:text-red-400"
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
            />
          )}
        </div>
      </section>
    </RoleGate>
  );
}
