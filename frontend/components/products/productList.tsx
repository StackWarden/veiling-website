"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import useGet from "../api/get";
import useDelete from "../api/delete";
import { RoleGate } from "../RoleGate";
import { SupplierName } from "../utility/SupplierName";
import List, { ListHeader } from "../list";
import CreateButton from "../createButton";
import useAuth from "../../hooks/useAuth";

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
  const { role } = useAuth();

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

  // Build headers array conditionally
  const headers: ListHeader[] = [
    { key: "species", label: "Species", align: "start" },
    { key: "supplier", label: "Supplier", align: "start" },
    { key: "quantity", label: "Quantity", align: "center" },
    { key: "price", label: "Price (€)", align: "center" },
  ];

  // Conditionally add Actions header for suppliers and admins
  if (role === "supplier" || role === "admin") {
    headers.push({ key: "actions", label: "Actions", align: "end" });
  }

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        {/* Header */}
        <div className="flex items-center w-full pt-8 pb-1">
          <div className="flex-1" />

          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Products
          </h1>

          <RoleGate allow={["supplier"]} fallback={<div className="flex-1 flex justify-end" />}>
            <CreateButton href="/products/create" label="Create Product" />
          </RoleGate>
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
          <List
            headers={headers}
            rows={products.map((p) => {
              const row: Record<string, unknown> = {
                id: p.id,
                species: p.species.title,
                supplier: <SupplierName supplierId={p.supplierId} />,
                quantity: p.quantity,
                price: `€${p.minPrice.toFixed(2)}`,
              };
              if (role === "supplier" || role === "admin") {
                row.actions = (
                  <RoleGate allow={["supplier", "admin"]}>
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
                  </RoleGate>
                );
              }
              return row;
            })}
            onRowClick={(row) => {
              window.location.href = `/products/info/${row.id}`;
            }}
            rowKey="id"
          />
        )}
      </div>
    </section>
  );
}
