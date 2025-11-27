"use client";

import React, { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import useGet from "../api/get";

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
  const { data: products, loading, error, execute, setData: setProducts } = useGet<Product>({
    route: "/products",
    autoFetch: false,
  });

  const fetchProducts = async () => {
    await execute();
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  return (
    <section className="bg-white rounded-xl shadow-lg p-6 w-full max-w-5xl border border-gray-200 mx-auto mt-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-800">Available Products</h2>

        <Link href="/products/create">
          <button
            className="p-2 rounded-full hover:bg-gray-100 transition flex items-center justify-center border border-gray-300"
            aria-label="Add product"
          >
            <svg
              width="26"
              height="26"
              viewBox="0 0 41 41"
              fill="#162218"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path d="M22.5499 8.20002C22.5499 7.06612 21.6338 6.15002 20.4999 6.15002C19.366 6.15002 18.4499 7.06612 18.4499 8.20002V18.45H8.1999C7.066 18.45 6.1499 19.3661 6.1499 20.5C6.1499 21.6339 7.066 22.55 8.1999 22.55H18.4499V32.8C18.4499 33.9339 19.366 34.85 20.4999 34.85C21.6338 34.85 22.5499 33.9339 22.5499 32.8V22.55H32.7999C33.9338 22.55 34.8499 21.6339 34.8499 20.5C34.8499 19.3661 33.9338 18.45 32.7999 18.45H22.5499V8.20002Z" />
            </svg>
          </button>
        </Link>
      </div>

      {loading ? (
        <p className="text-gray-500 text-center py-4">Loading products...</p>
      ) : products.length === 0 ? (
        <p className="text-gray-500 text-center py-4">No products available.</p>
      ) : (
        <div className="overflow-hidden rounded-lg border border-gray-300">
          <table className="w-full text-left">
            <thead className="bg-gray-100 text-gray-700">
              <tr>
                <th className="p-3 border-b">Species</th>
                <th className="p-3 border-b">Pot Size</th>
                <th className="p-3 border-b">Stem Length (cm)</th>
                <th className="p-3 border-b">Quantity</th>
                <th className="p-3 border-b">Price (€)</th>
                <th className="p-3 border-b">Location</th>
                <th className="p-3 border-b">Auction Date</th>
                <th className="p-3 border-b text-center">Photo</th>
              </tr>
            </thead>

            <tbody className="divide-y divide-gray-200">
              {products.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50 transition">
                  <td className="p-3">{p.species}</td>
                  <td className="p-3">{p.potSize}</td>
                  <td className="p-3">{p.stemLength}</td>
                  <td className="p-3">{p.quantity}</td>
                  <td className="p-3 font-semibold">€{p.minPrice.toFixed(2)}</td>
                  <td className="p-3">{getClockLocationName(p.clockLocation)}</td>
                  <td className="p-3">{p.auctionDate ?? "-"}</td>
                  <td className="p-3 text-center">
                    {p.photoUrl ? (
                      <Image
                        src={p.photoUrl}
                        alt={p.species}
                        width={48}
                        height={48}
                        className="w-12 h-12 object-cover rounded-md mx-auto shadow-sm"
                      />
                    ) : (
                      <span className="text-gray-400 text-sm">No photo</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
