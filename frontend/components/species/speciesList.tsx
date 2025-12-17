"use client";

import React, { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import useGet from "../api/get";
import useDelete from "../api/delete";
import { RoleGate } from "../RoleGate";

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

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        {/* Header */}
        <div className="flex items-center mb-6 w-full pt-8 pb-4">
          <div className="flex-1" />

          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Species
          </h1>

          <div className="flex-1 flex justify-end">
            <RoleGate allow={["admin"]} fallback={<div />}>
              <Link href="/species/create">
                <p className="flex items-center gap-2 p-1 rounded-full hover:cursor-pointer">
                  <span className="text-[#162218] font-medium">
                    Create Species
                  </span>
                  <Image
                    src="/images/Plus.svg"
                    alt="Create Species"
                    width={40}
                    height={40}
                  />
                </p>
              </Link>
            </RoleGate>
          </div>
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
          <div className="overflow-hidden rounded-xl border border-[#D9D9D9] p-4">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="text-[#4D4D4D]">
                  <th className="p-3 text-start">Title</th>
                  <th className="p-3 text-start">Latin Name</th>
                  <th className="p-3 text-start">Family</th>
                  <th className="p-3 text-center">Perennial</th>
                  <RoleGate allow={["admin"]}>
                    <th className="p-3 text-end">Actions</th>
                  </RoleGate>
                </tr>
              </thead>

              <tbody className="text-[#1A1A1A]">
                {species.map((s) => (
                  <tr
                    key={s.id}
                    className="hover:bg-[#162218] hover:text-white transition cursor-pointer"
                    onClick={() =>
                      (window.location.href = `/species/edit/${s.id}`)
                    }
                  >
                    <td className="p-4 rounded-l-2xl">
                      {s.title}
                    </td>

                    <td className="p-4">
                      {s.latinName ?? "—"}
                    </td>

                    <td className="p-4">
                      {s.family ?? "—"}
                    </td>

                    <td className="p-4 text-center">
                      {s.isPerennial ? "Yes" : "No"}
                    </td>

                    <RoleGate allow={["admin"]}>
                      <td className="p-4 text-end rounded-r-2xl">
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
                      </td>
                    </RoleGate>
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
