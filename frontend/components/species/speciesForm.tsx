"use client";

import { useEffect, useState } from "react";
import { usePost } from "@/components/api/post";
import { usePut } from "@/components/api/put";
import { Species } from "@/components/types/species";

type Props = {
  existing?: Species;
  onSaved?: () => void;
};

export default function SpeciesForm({ existing, onSaved }: Props) {
  const [form, setForm] = useState({
    title: "",
    latinName: "",
    family: "",
    growthType: "",
    description: "",
    isPerennial: false,
  });

  useEffect(() => {
    if (existing) {
      setForm({
        title: existing.title,
        latinName: existing.latinName ?? "",
        family: existing.family ?? "",
        growthType: existing.growthType ?? "",
        description: existing.description ?? "",
        isPerennial: existing.isPerennial,
      });
    }
  }, [existing]);

  const { execute: createSpecies, loading: creating } = usePost({
    route: "/species",
    onSuccess: onSaved,
  });

  const { execute: updateSpecies, loading: updating } = usePut({
    baseRoute: "/species",
    onSuccess: onSaved,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (existing) {
      await updateSpecies(existing.id, form);
    } else {
      await createSpecies(form);
    }
  };

  return (
    <div className="flex justify-center items-start px-6 pt-8">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-3xl space-y-8"
      >
        <h1 className="text-3xl font-bold text-start">
          {existing ? "Edit Species" : "Create Species"}
        </h1>

        {/* two column layout */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* LEFT */}
          <div className="space-y-4">
            <div>
              <label className="font-semibold">Title</label>
              <input
                type="text"
                value={form.title}
                onChange={(e) =>
                  setForm({ ...form, title: e.target.value })
                }
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                required
              />
            </div>

            <div>
              <label className="font-semibold">Latin name</label>
              <input
                type="text"
                value={form.latinName}
                onChange={(e) =>
                  setForm({ ...form, latinName: e.target.value })
                }
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
              />
            </div>

            <div>
              <label className="font-semibold">Family</label>
              <input
                type="text"
                value={form.family}
                onChange={(e) =>
                  setForm({ ...form, family: e.target.value })
                }
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
              />
            </div>
          </div>

          {/* RIGHT */}
          <div className="space-y-4">
            <div>
              <label className="font-semibold">Growth type</label>
              <input
                type="text"
                value={form.growthType}
                onChange={(e) =>
                  setForm({ ...form, growthType: e.target.value })
                }
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
              />
            </div>

            <div>
              <label className="font-semibold">Description</label>
              <textarea
                value={form.description}
                onChange={(e) =>
                  setForm({ ...form, description: e.target.value })
                }
                className="mt-1 w-full border border-gray-300 rounded-lg p-2 min-h-[96px]"
              />
            </div>

            <div className="flex items-center gap-3 mt-2">
              <input
                type="checkbox"
                checked={form.isPerennial}
                onChange={(e) =>
                  setForm({
                    ...form,
                    isPerennial: e.target.checked,
                  })
                }
              />
              <label className="font-semibold">Perennial</label>
            </div>
          </div>
        </div>

        {/* BUTTON */}
        <div className="flex w-full">
          <button
            type="submit"
            disabled={creating || updating}
            className="w-full bg-[#162218] text-white py-3 rounded-lg font-semibold hover:bg-[#0f1c14] transition"
          >
            {existing
              ? updating
                ? "Saving..."
                : "Save changes"
              : creating
              ? "Creating..."
              : "Create Species"}
          </button>
        </div>
      </form>
    </div>
  );
}
