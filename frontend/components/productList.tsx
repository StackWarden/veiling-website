"use client";

import React, { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";

type Product = {
  id: string;
  supplierId: string;
  species: string;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  clockLocation: string | number;
  auctionDate?: string | null;
  photoUrl?: string | null;
};

const getClockLocationName = (value: string | number) => {
  const mapping: Record<number, string> = {
    0: "Naaldwijk",
    1: "Aalsmeer",
    2: "Rijnsburg",
    3: "Eelde",
  };
  if (typeof value === "string" && isNaN(Number(value))) return value;
  return mapping[Number(value)] ?? "Onbekend";
};

export default function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);

  

  const fetchProducts = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/products`);
      const data = await res.json();
      setProducts(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("Error fetching products:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this product?")) return;

    try {
      await fetch(`${process.env.NEXT_PUBLIC_API_URL}/products/${id}`, {
        method: "DELETE",
      });

    
      setProducts((prev) => prev.filter((p) => p.id !== id));

    } catch (err) {
      console.error("Failed to delete product:", err);
    }
  };

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">

        {/* add product button */}
        <div className="relative w-full max-w-[90rem] px-4 mb-6 flex items-end">
            
            {/* Centered title */}
            <h1 className="absolute left-1/2 -translate-x-1/2 text-[32px] font-bold text-[#162218]">
              Products
            </h1>

            {/* Add product button (right aligned) */}
            <div className="ml-auto">
              <Link href="/products/create">
                <button 
                  aria-label="Add Product"
                  className="bg-[#0F1C14] text-white w-10 h-10 rounded-md text-xl font-bold"
                >
                  +
                </button>
              </Link>
            </div>

          </div>
      {loading ? (
        <p className="text-gray-500 text-center py-6">Loading products...</p>
      ) : products.length === 0 ? (
        <p className="text-gray-500 text-center py-6">No products available.</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-[D9D9D9]">
          <table className="w-full table-fixed border-collapse text-left">

            {/* tabel header */}
            <thead className="bg-white">
              <tr className="border-b border-[#D9D9D9] text-[#162218]">
                <th className="py-3 text-center w-1/6">Species</th>
                <th className="py-3 text-center w-1/6">Quantity</th>
                <th className="py-3 text-center w-1/6">Price (€)</th>
                <th className="py-3 text-center w-1/6">Location</th>
                <th className="py-3 text-center w-1/6">Auction Date</th>
                <th className="py-3 text-center w-1/6">Actions</th>
              </tr>
            </thead>

            {/* tabel body */}
            <tbody className="bg-white text-[#1A1A1A]">
              {products.map((p) => (
                <tr
                  key={p.id}
                  className="border-b rounded-md border-[#E5E5E5] hover:bg-[#162218] hover:text-white transition"
                >
                  <td className="py-4 text-center truncate rounded-l-lg">{p.species}</td>
                  <td className="py-4 text-center truncate">{p.quantity}</td>
                  <td className="py-4 text-center truncate">€{p.minPrice.toFixed(2)}</td>
                  <td className="py-4 text-center truncate">{getClockLocationName(p.clockLocation)}</td>
                  <td className="py-4 text-center truncate">{p.auctionDate ?? "-"}</td>

                  <td className="py-4 rounded-r-lg">
                    <div className="flex gap-6 justify-center">
                      <Link
                        href={`/products/info/${p.id}`}
                        className="hover:underline underline-offset-2"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => handleDelete(p.id)}
                        className="hover:underline underline-offset-2 text-red-600 hover:text-red-400"
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