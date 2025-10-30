"use client";

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

export default function ProductTable({ products }: { products: Product[] }) {
  return (
    <>
    <div className="flex justify-between items-center mb-4">   
      <h2 className="text-xl font-semibold">Available Products</h2>

      <Link
          className="p-1 hover:bg-gray-100 rounded-full"
          aria-label="Add product"
          href="products/create"
        >
          <svg
            width="30"
            height="30"
            viewBox="0 0 41 41"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M22.5499 8.20002C22.5499 7.06612 21.6338 6.15002 20.4999 6.15002C19.366 6.15002 18.4499 7.06612 18.4499 8.20002V18.45H8.1999C7.066 18.45 6.1499 19.3661 6.1499 20.5C6.1499 21.6339 7.066 22.55 8.1999 22.55H18.4499V32.8C18.4499 33.9339 19.366 34.85 20.4999 34.85C21.6338 34.85 22.5499 33.9339 22.5499 32.8V22.55H32.7999C33.9338 22.55 34.8499 21.6339 34.8499 20.5C34.8499 19.3661 33.9338 18.45 32.7999 18.45H22.5499V8.20002Z"
              fill="black"
            />
          </svg>
        </Link>
      </div>

      {products.length === 0 ? (
        <p className="text-gray-500">No products available.</p>
      ) : (
        <table className="w-full border-collapse border border-gray-300 text-left">
          <thead className="bg-gray-100">
            <tr>
              <th className="border border-gray-300 p-2">Species</th>
              <th className="border border-gray-300 p-2">Pot Size</th>
              <th className="border border-gray-300 p-2">Stem Length (cm)</th>
              <th className="border border-gray-300 p-2">Quantity</th>
              <th className="border border-gray-300 p-2">Price (€)</th>
              <th className="border border-gray-300 p-2">Location</th>
              <th className="border border-gray-300 p-2">Auction Date</th>
              <th className="border border-gray-300 p-2">Photo</th>
            </tr>
          </thead>
          <tbody>
            {products.map((p) => (
              <tr key={p.id} className="hover:bg-gray-50">
                <td className="border border-gray-300 p-2">{p.species}</td>
                <td className="border border-gray-300 p-2">{p.potSize}</td>
                <td className="border border-gray-300 p-2">{p.stemLength}</td>
                <td className="border border-gray-300 p-2">{p.quantity}</td>
                <td className="border border-gray-300 p-2">
                  €{p.minPrice.toFixed(2)}
                </td>
                <td className="border border-gray-300 p-2">
                  {getClockLocationName(p.clockLocation)}
                </td>
                <td className="border border-gray-300 p-2">
                  {p.auctionDate ?? "-"}
                </td>
                <td className="border border-gray-300 p-2 text-center">
                  {p.photoUrl ? (
                    <img
                      src={p.photoUrl}
                      alt={p.species}
                      className="w-12 h-12 object-cover rounded mx-auto"
                    />
                  ) : (
                    <span className="text-gray-400">No photo</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      </>
  );
}
