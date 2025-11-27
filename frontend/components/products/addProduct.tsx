"use client";

import { useState } from "react";
import { usePost } from "../api/post";

enum ClockLocation {
  Aalsmeer = "Aalsmeer",
  Naaldwijk = "Naaldwijk",
  Rijnsburg = "Rijnsburg",
}

type Product = {
  species: string;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  clockLocation: ClockLocation;
  photoUrl: string;
};

export default function AddProduct() {
  const [species, setSpecies] = useState("");
  const [potHeight, setPotHeight] = useState("");
  const [potDiameter, setPotDiameter] = useState("");
  const [stemLength, setStemLength] = useState("");
  const [quantity, setQuantity] = useState("");
  const [minPrice, setMinPrice] = useState("");
  const [clockLocation, setClockLocation] = useState(ClockLocation.Aalsmeer);
  const [photo, setPhoto] = useState<File | null>(null);

  const [success, setSuccess] = useState(false);
  const { loading, error, execute: createProduct } = usePost<Product>({
    route: "/products",

    onSuccess: () => {
      console.log("Product created!");
      setSuccess(true);   // <-- your own success flag
    },

    onError: (err) => {
      console.error("Error creating product:", err);
      setSuccess(false);
    }
  });
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const potSize = `${potHeight}mm x ${potDiameter}mm`;

    await createProduct({
      species,
      potSize,
      stemLength: Number(stemLength),
      quantity: Number(quantity),
      minPrice: Number(minPrice),
      clockLocation,
      photoUrl: photo ? photo.name : "",
    });
  };
  return (
    <div className="flex justify-center items-start px-6 pt-8">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-3xl space-y-6"
      >
        <h1 className="text-3xl font-bold text-start">Create Product</h1>

        {success && (
          <p className="text-green-600 text-center font-semibold">
            Product added successfully
          </p>
        )}
        {error && (
          <p className="text-red-600 text-center font-semibold">{error}</p>
        )}

        {/* Two-column layout */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

          {/* LEFT SIDE */}
          <div className="space-y-4">
            <div>
              <label className="font-semibold">Species</label>
              <input
                type="text"
                onChange={(e) => setSpecies(e.target.value)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                required
              />
            </div>

            <div>
              <label className="font-semibold">Stem Length (cm)</label>
              <input
                type="number"
                onChange={(e) => setStemLength(e.target.value)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                required
              />
            </div>

            <div>
              <label className="font-semibold">Minimum Price (€)</label>
              <input
                type="number"
                step="0.01"
                onChange={(e) => setMinPrice(e.target.value)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                required
              />
            </div>
          </div>

          {/* RIGHT SIDE */}
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="font-semibold">Pot height (mm)</label>
                <input
                  type="number"
                  onChange={(e) => setPotHeight(e.target.value)}
                  className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                  required
                />
              </div>

              <div>
                <label className="font-semibold">Pot diameter (mm)</label>
                <input
                  type="number"
                  onChange={(e) => setPotDiameter(e.target.value)}
                  className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                  required
                />
              </div>
            </div>

            <div>
              <label className="font-semibold">Quantity</label>
              <input
                type="number"
                onChange={(e) => setQuantity(e.target.value)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
                required
              />
            </div>

            <div>
              <label className="font-semibold">Clock Location</label>
              <select
                value={clockLocation}
                onChange={(e) =>
                  setClockLocation(e.target.value as ClockLocation)
                }
                className="mt-1 w-full border border-gray-300 rounded-lg p-2 bg-white"
              >
                {Object.values(ClockLocation).map((loc) => (
                  <option key={loc} value={loc}>
                    {loc}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="font-semibold">Upload photo</label>
              <input
                type="file"
                accept="image/*"
                onChange={(e) => setPhoto(e.target.files?.[0] || null)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
              />
            </div>
          </div>
        </div>

        {/* BUTTON */}
        <div className="flex w-full">
          <button
            type="submit"
            className="w-full bg-[#162218] text-white py-3 rounded-lg font-semibold hover:bg-[#0f1c14] transition"
          >
            Submit
          </button>
        </div>
      </form>
    </div>
  );
}
