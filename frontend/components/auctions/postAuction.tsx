"use client";

import { useEffect, useState } from "react";
import { usePost } from "../api/post";
import useGet from "@/components/api/get";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { SupplierName } from "../utility/SupplierName";

/* ---------- Types ---------- */

type Species = {
  id: string;
  title: string;
};

type Product = {
  id: string;
  supplierId: string;
  species: Species;
  photoUrl?: string | null;
};

interface Auction {
  description: string;
  startTime: string;
  endTime: string;
  productIds: string[];
}
/* ---------- Component ---------- */

export default function PostAuction() {
  const router = useRouter();

  const {
    loading: postLoading,
    error: postError,
    execute: createAuction,
  } = usePost<Auction>({
    route: "/auctions",
    onSuccess: () => {
      router.push(
        "/auctions?success=" +
          encodeURIComponent("Auction aangemaakt!")
      );
    },
  });

  const [products, setProducts] = useState<Product[]>([]);
  const { loading: getLoading, execute: fetchProducts } = useGet<Product>({
    route: "/products",
    autoFetch: false,
    onSuccess: (data) => setProducts(data),
  });

  const [auction, setAuction] = useState<Auction>({
    description: "",
    startTime: new Date().toISOString(),
    endTime: new Date(
      Date.now() + 7 * 24 * 60 * 60 * 1000
    ).toISOString(),
    productIds: [],
  });

  /* ---------- Helpers ---------- */

  const handleChange = (
    key: keyof Auction,
    value: string | string[]
  ) => {
    setAuction((prev) => ({ ...prev, [key]: value }));
  };

  const toggleProduct = (id: string) => {
    setAuction((prev) => ({
      ...prev,
      productIds: prev.productIds.includes(id)
        ? prev.productIds.filter((p) => p !== id)
        : [...prev.productIds, id],
    }));
  };

  const selectAll = () => {
    setAuction((prev) => ({
      ...prev,
      productIds: products.map((p) => p.id),
    }));
  };

  const deselectAll = () => {
    setAuction((prev) => ({
      ...prev,
      productIds: [],
    }));
  };

  const handleSubmit = async (
    e: React.FormEvent<HTMLFormElement>
  ) => {
    e.preventDefault();
    await createAuction(auction);
  };

  /* ---------- Effects ---------- */

  useEffect(() => {
    fetchProducts();
  }, []);

  /* ---------- UI ---------- */

  return (
    <div className="flex justify-center items-start px-6 pt-8">
      <form onSubmit={handleSubmit} className="w-full max-w-4xl space-y-8">
        <h1 className="text-3xl font-bold text-center">Create Auction</h1>

        {/* Description */}
        <div className="flex flex-col">
          <label className="font-semibold">Auction Description</label>
          <textarea
            value={auction.description}
            onChange={(e) =>
              handleChange("description", e.target.value)
            }
            className="mt-1 w-full border rounded-lg p-3 min-h-[100px]"
          />
        </div>

        {/* Times */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div className="flex flex-col">
            <label className="font-semibold">Start Time</label>
            <input
              type="datetime-local"
              value={auction.startTime.slice(0, 16)}
              onChange={(e) =>
                handleChange("startTime", e.target.value)
              }
              className="mt-1 w-full border rounded-lg p-2"
            />
          </div>

          <div className="flex flex-col">
            <label className="font-semibold">End Time</label>
            <input
              type="datetime-local"
              value={auction.endTime.slice(0, 16)}
              onChange={(e) =>
                handleChange("endTime", e.target.value)
              }
              className="mt-1 w-full border rounded-lg p-2"
            />
          </div>
        </div>

        {/* Products */}
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-bold">Select Products</h2>
            <div className="flex gap-4">
              <button
                type="button"
                onClick={selectAll}
                className="text-sm underline"
              >
                Select all
              </button>
              <button
                type="button"
                onClick={deselectAll}
                className="text-sm underline"
              >
                Deselect all
              </button>
            </div>
          </div>

          {getLoading ? (
            <p>Loading products...</p>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {products.map((p) => (
                <label
                  key={p.id}
                  className="flex items-center gap-4 border p-3 rounded-lg cursor-pointer"
                >
                  <input
                    type="checkbox"
                    checked={auction.productIds.includes(p.id)}
                    onChange={() => toggleProduct(p.id)}
                  />

                  <Image
                    src={p.photoUrl || "/images/placeholder.jpg"}
                    alt={p.species.title}
                    width={64}
                    height={64}
                    className="object-cover rounded"
                  />

                  <div>
                    <p className="font-semibold">
                      {p.species.title}
                    </p>
                    <p className="text-sm text-gray-600">
                      Supplier: <SupplierName supplierId={p.supplierId} />
                    </p>
                  </div>
                </label>
              ))}
            </div>
          )}
        </div>

        {/* Submit */}
        <div className="flex justify-center">
          <button
            type="submit"
            disabled={postLoading}
            className="w-48 bg-[#162218] text-white py-3 rounded-lg font-semibold hover:bg-[#0f1c14]"
          >
            {postLoading ? "Posting..." : "Create Auction"}
          </button>
        </div>

        {postError && (
          <p className="text-red-600 text-center font-semibold">
            {postError}
          </p>
        )}
      </form>
    </div>
  );
}