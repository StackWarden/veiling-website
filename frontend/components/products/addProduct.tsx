"use client";

import { useState } from "react";
import { usePost } from "../api/post";
import useGet from "../api/get";

/* ---------- Types ---------- */

type Species = {
  id: string;
  title: string;
};

type CreateProductPayload = {
  speciesId: string;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  photoUrl?: string | null;
};

export default function AddProduct() {
  /* ---------- State ---------- */
  const [speciesId, setSpeciesId] = useState<string>("");
  const [potHeight, setPotHeight] = useState("");
  const [potDiameter, setPotDiameter] = useState("");
  const [stemLength, setStemLength] = useState("");
  const [quantity, setQuantity] = useState("");
  const [minPrice, setMinPrice] = useState("");
  const [photo, setPhoto] = useState<File | null>(null);
  const [success, setSuccess] = useState(false);

  /* ---------- GET Species ---------- */
  const {
    data: speciesList,
    loading: speciesLoading,
    error: speciesError,
  } = useGet<Species>({
    route: "/species",
  });

  /* ---------- POST Product ---------- */
  const { error, execute: createProduct } = usePost<CreateProductPayload>({
    route: "/products",
    onSuccess: () => setSuccess(true),
    onError: () => setSuccess(false),
  });

  /* ---------- Submit ---------- */
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!speciesId) return;

    const potSize = `${potHeight}mm x ${potDiameter}mm`;

    await createProduct({
      speciesId,
      potSize,
      stemLength: Number(stemLength),
      quantity: Number(quantity),
      minPrice: Number(minPrice),
      photoUrl: photo ? photo.name : null,
    });
  };

  /* ---------- UI ---------- */
  return (
    <div className="flex justify-center items-start px-6 pt-10">
      <form onSubmit={handleSubmit} className="w-full max-w-3xl space-y-10">
        <h1 className="text-3xl font-bold">Create Product</h1>

        {success && (
          <p className="text-green-600 font-semibold">
            Product added successfully
          </p>
        )}

        {(error || speciesError) && (
          <p className="text-red-600 font-semibold">
            {error || speciesError}
          </p>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
          {/* LEFT */}
          <div className="space-y-6">
            <div className="space-y-2">
              <label className="font-semibold">Species</label>
              <select
                value={speciesId}
                onChange={(e) => setSpeciesId(e.target.value)}
                className="w-full border border-gray-300 rounded-lg p-3 bg-white"
                required
              >
                <option value="" disabled>
                  {speciesLoading ? "Loading species..." : "Select species"}
                </option>

                {speciesList.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.title}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <label className="font-semibold">Stem Length (cm)</label>
              <input
                type="number"
                onChange={(e) => setStemLength(e.target.value)}
                className="w-full border border-gray-300 rounded-lg p-3"
                required
              />
            </div>

            <div className="space-y-2">
              <label className="font-semibold">Minimum Price (€)</label>
              <input
                type="number"
                step="0.01"
                onChange={(e) => setMinPrice(e.target.value)}
                className="w-full border border-gray-300 rounded-lg p-3"
                required
              />
            </div>
          </div>

          {/* RIGHT */}
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="font-semibold">Pot height (mm)</label>
                <input
                  type="number"
                  onChange={(e) => setPotHeight(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg p-3"
                  required
                />
              </div>

              <div className="space-y-2">
                <label className="font-semibold">Pot diameter (mm)</label>
                <input
                  type="number"
                  onChange={(e) => setPotDiameter(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg p-3"
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="font-semibold">Quantity</label>
              <input
                type="number"
                onChange={(e) => setQuantity(e.target.value)}
                className="w-full border border-gray-300 rounded-lg p-3"
                required
              />
            </div>

            <div className="space-y-2">
              <label className="font-semibold">Upload photo</label>
              <input
                type="file"
                accept="image/*"
                onChange={(e) => setPhoto(e.target.files?.[0] || null)}
                className="w-full border border-gray-300 rounded-lg p-3"
              />
            </div>
          </div>
        </div>

        <button
          type="submit"
          className="w-full mt-6 bg-[#162218] text-white py-4 rounded-lg font-semibold hover:bg-[#0f1c14] transition"
        >
          Submit
        </button>
      </form>
    </div>
  );
}
