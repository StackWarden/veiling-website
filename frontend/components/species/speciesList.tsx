"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import useGet from "../api/get";
import useDelete from "../api/delete";
import { RoleGate } from "../RoleGate";
import List, { ListHeader } from "../list";
import CreateButton from "../createButton";
import useAuth from "../../hooks/useAuth";

/* ---------- Types ---------- */

type Species = {
  id: string;
  title: string;
  latinName?: string;
  family?: string;
  isPerennial: boolean;
};

/* ---------- Component ---------- */

export default function SpeciesList() {
  const [species, setSpecies] = useState<Species[]>([]);
  const { role } = useAuth();

  const { loading, execute } = useGet<Species>({
    route: "/species",
    autoFetch: false,
    onSuccess: (data) => setSpecies(data),
  });

  const { loading: deleting, execute: deleteSpecies } = useDelete({
    baseRoute: "/species",
    onSuccess: (id) =>
      setSpecies((prev) => prev.filter((s) => s.id !== id)),
  });

  useEffect(() => {
    execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure?")) return;
    await deleteSpecies(id);
  };

  // Build headers array conditionally
  const headers: ListHeader[] = [
    { key: "title", label: "Title", align: "start" },
    { key: "latinName", label: "Latin Name", align: "start" },
    { key: "family", label: "Family", align: "start" },
    { key: "perennial", label: "Perennial", align: "center" },
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
            Species
          </h1>

          <RoleGate allow={["admin"]} fallback={<div className="flex-1 flex justify-end" />}>
            <CreateButton href="/species/create" label="Create Species" />
          </RoleGate>
        </div>

        {/* Content */}
        {loading || deleting ? (
          <p className="text-gray-500 text-center py-6">
            Loading species...
          </p>
        ) : species.length === 0 ? (
          <p className="text-gray-500 text-center py-6">
            No species available.
          </p>
        ) : (
          <List
            headers={headers}
            rows={species.map((s) => {
              const row: Record<string, unknown> = {
                id: s.id,
                title: s.title,
                latinName: s.latinName ?? "—",
                family: s.family ?? "—",
                perennial: s.isPerennial ? "Yes" : "No",
              };
              if (role === "admin") {
                row.actions = (
                  <RoleGate allow={["admin"]}>
                    <div className="flex gap-6 justify-end">
                      <Link
                        href={`/species/edit/${s.id}`}
                        onClick={(e) => e.stopPropagation()}
                        className="hover:underline"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDelete(s.id);
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
            onRowClick={(row) => {
              window.location.href = `/species/edit/${row.id}`;
            }}
            rowKey="id"
          />
        )}
      </div>
    </section>
  );
}
