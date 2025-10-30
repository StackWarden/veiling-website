"use client";

import { useState } from "react";
import { usePostData } from "./api/post";

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
  const { loading, error, success, postData } = usePostData<Product>("/products");

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const potSize = `${potHeight}mm x ${potDiameter}mm`;

    const product: Product = {
        species,
        potSize,
        stemLength: Number(stemLength),
        quantity: Number(quantity),
        minPrice: Number(minPrice),
        clockLocation,
        photoUrl: photo ? photo.name : "",
    };

    await postData(product)
  };

  return (
    <>
      <h1 className="text-2xl font-semibold text-center mb-6">Create Product</h1>

      <form onSubmit={handleSubmit} className="flex flex-col space-y-4">
        {success && <p className="text-green-600 text-center">Product added successfully</p>}
        {error && <p className="text-red-600 text-center">{error}</p>}

        <div className="flex flex-col">
          <label htmlFor="species" className="text-gray-700 mb-1">Species</label>
          <input
            type="text"
            id="species"
            onChange={(e) => setSpecies(e.target.value)}
            placeholder="Enter the species"
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col">
            <label htmlFor="potHeight" className="text-gray-700 mb-1">Pot height (mm)</label>
            <input
              type="number"
              id="potHeight"
              onChange={(e) => setPotHeight(e.target.value)}
              className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          <div className="flex flex-col">
            <label htmlFor="potDiameter" className="text-gray-700 mb-1">Pot diameter (mm)</label>
            <input
              type="number"
              id="potDiameter"
              onChange={(e) => setPotDiameter(e.target.value)}
              className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
        </div>

        <div className="flex flex-col">
          <label htmlFor="stemLength" className="text-gray-700 mb-1">Stem Length (cm)</label>
          <input
            type="number"
            id="stemLength"
            onChange={(e) => setStemLength(e.target.value)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div className="flex flex-col">
          <label htmlFor="quantity" className="text-gray-700 mb-1">Quantity</label>
          <input
            type="number"
            id="quantity"
            onChange={(e) => setQuantity(e.target.value)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div className="flex flex-col">
          <label htmlFor="minPrice" className="text-gray-700 mb-1">Minimum Price (€)</label>
          <input
            type="number"
            step="0.01"
            id="minPrice"
            onChange={(e) => setMinPrice(e.target.value)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div className="flex flex-col">
          <label htmlFor="clockLocation" className="text-gray-700 mb-1">Clock Location</label>
          <select
            id="clockLocation"
            value={clockLocation}
            onChange={(e) => setClockLocation(e.target.value as ClockLocation)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
          >
            {Object.values(ClockLocation).map((loc) => (
              <option key={loc} value={loc}>{loc}</option>
            ))}
          </select>
        </div>

        <div className="flex flex-col">
          <label htmlFor="photo" className="text-gray-700 mb-1">Upload Photo</label>
          <input
            type="file"
            id="photo"
            accept="image/*"
            onChange={(e) => setPhoto(e.target.files?.[0] || null)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <button
          type="submit"
          className="mt-4 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors"
        >
          Submit
        </button>
      </form>
    </>
  );
}