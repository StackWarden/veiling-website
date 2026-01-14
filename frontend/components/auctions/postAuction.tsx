"use client";

import { useCallback, useEffect, useState } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";

import { usePost } from "../api/post";
import useGet from "@/components/api/get";
import Form, { FormField } from "@/components/form";
import { SupplierName } from "../utility/SupplierName";


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
  auctionDate: string; // "YYYY-MM-DD"
  auctionTime: string | null; // "HH:mm" or null
  productIds: string[];
}


function defaultAuctionDate() {
  // local date -> "YYYY-MM-DD"
  const d = new Date();
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}


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
        "/auctions?success=" + encodeURIComponent("Auction aangemaakt!")
      );
    },
  });

  const [products, setProducts] = useState<Product[]>([]);
  const handleProductsLoaded = useCallback(
    (data: Product[]) => setProducts(data),
    []
  );

  const { loading: getLoading, execute: fetchProducts } = useGet<Product>({
    route: "/products",
    autoFetch: false,
    onSuccess: handleProductsLoaded,
  });

  const [auction, setAuction] = useState<Auction>({
    description: "",
    auctionDate: defaultAuctionDate(),
    auctionTime: null,
    productIds: [],
  });

  useEffect(() => {
    async function load() {
      try {
        await fetchProducts();
      } catch (e) {
        console.error(e);
      }
    }
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fields: Array<FormField<Auction>> = [
    {
      name: "description",
      label: "Auction Description",
      type: "textarea",
      colSpan: 2,
    },
    {
      name: "auctionDate",
      label: "Auction Date",
      type: "date",
      colSpan: 1,
    },
    {
      name: "auctionTime",
      label: "Auction Time (optional)",
      type: "time",
      colSpan: 1,
      // input type="time" wil een string; als null -> ""
      formatValue: (value) => (value ? String(value) : ""),
      // als leeg -> null
      parseValue: (raw) => (raw && raw.trim().length > 0 ? raw : null),
    },
    {
      name: "productIds",
      label: "Select Products",
      type: "custom",
      colSpan: 2,
      render: ({ value, setValue }) => {
        const selectedIds = Array.isArray(value) ? (value as string[]) : [];

        const toggleProduct = (id: string) => {
          setValue(
            "productIds",
            selectedIds.includes(id)
              ? selectedIds.filter((p) => p !== id)
              : [...selectedIds, id]
          );
        };

        const selectAll = () => setValue("productIds", products.map((p) => p.id));
        const deselectAll = () => setValue("productIds", []);

        return (
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <h2 className="text-xl font-bold">Select Products</h2>
              <div className="flex gap-4">
                <button type="button" onClick={selectAll} className="text-sm underline">
                  Select all
                </button>
                <button type="button" onClick={deselectAll} className="text-sm underline">
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
                      checked={selectedIds.includes(p.id)}
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
                      <p className="font-semibold">{p.species.title}</p>
                      <p className="text-sm text-gray-600">
                        Supplier: <SupplierName supplierId={p.supplierId} />
                      </p>
                    </div>
                  </label>
                ))}
              </div>
            )}
          </div>
        );
      },
    },
  ];

  return (
    <Form<Auction>
      title="Create Auction"
      values={auction}
      setValues={setAuction}
      fields={fields}
      columns={2}
      submitting={postLoading}
      submitLabel="Create Auction"
      error={postError}
      onSubmit={async (values) => {
        await createAuction(values);
      }}
    />
  );
}
