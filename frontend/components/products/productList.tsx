"use client";

import React, { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import useGet from "../api/get";
import useDelete from "../api/delete";
import { RoleGate } from "../RoleGate";
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
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  photoUrl?: string | null;
};

/* ---------- Component ---------- */

export default function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);

  const { loading, execute } = useGet<Product>({
    route: "/products",
    autoFetch: false,
    onSuccess: (data) => setProducts(data),
  });

  const { loading: deleting, execute: deleteProduct } = useDelete({
    baseRoute: "/products",
    onSuccess: (id) => {
      setProducts((prev) => prev.filter((p) => p.id !== id));
    },
  });

  useEffect(() => {
    execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure?")) return;
    await deleteProduct(id);
  };

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        {/* Header */}
        <div className="flex items-center mb-6 w-full pt-8 pb-4">
          <div className="flex-1" />

          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Products
          </h1>

          <div className="flex-1 flex justify-end">
            <RoleGate allow={["supplier"]} fallback={<div />}>
              <Link href="/products/create">
                <p className="flex items-center gap-2 p-1 rounded-full hover:cursor-pointer">
                  <span className="text-[#162218] font-medium">
                    Create Product
                  </span>
                  <Image
                    src="/images/Plus.svg"
                    alt="Create Product"
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
            Loading products...
          </p>
        ) : products.length === 0 ? (
          <p className="text-gray-500 text-center py-6">
            No products available.
          </p>
        ) : (
          <div className="overflow-hidden rounded-xl border border-[#D9D9D9] p-4">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="text-[#4D4D4D]">
                  <th className="p-3 text-start">Species</th>
                  <th className="p-3 text-start">Supplier</th>
                  <th className="p-3 text-center">Quantity</th>
                  <th className="p-3 text-center">Price (€)</th>
                  <RoleGate allow={["supplier"]}>
                    <th className="p-3 text-end">Actions</th>
                  </RoleGate>
                </tr>
              </thead>

              <tbody className="text-[#1A1A1A]">
                {products.map((p) => (
                  <tr
                    key={p.id}
                    className="hover:bg-[#162218] hover:text-white transition cursor-pointer"
                    onClick={() =>
                      (window.location.href = `/products/info/${p.id}`)
                    }
                  >
                    <td className="p-4 rounded-l-2xl">
                      {p.species.title}
                    </td>
                    <td className="p-4 rounded-l-2xl">
                      <SupplierName supplierId={p.supplierId} />
                    </td>
                    <td className="p-4 text-center">{p.quantity}</td>

                    <td className="p-4 text-center">
                      €{p.minPrice.toFixed(2)}
                    </td>

                    <RoleGate allow={["supplier"]}>
                      <td className="p-4 text-end rounded-r-2xl">
                        <div className="flex gap-6 justify-end">
                          <Link
                            href={`/products/edit/${p.id}`}
                            onClick={(e) => e.stopPropagation()}
                            className="hover:underline"
                          >
                            Edit
                          </Link>

                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDelete(p.id);
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