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

type ClockLocation = {
  id: string;
  name: string;
};

interface Auction {
  auctionneerId: string;
  description: string;
  auctionDate: string; // "YYYY-MM-DD"
  auctionTime: string | null; // "HH:mm" or null
  productIds: string[];
  clockLocationId?: string | null;
}


function defaultAuctionDate() {
  // local date -> "YYYY-MM-DD"
  const d = new Date();
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}


export default function PostAuction() {
  const router = useRouter();
  const [userId, setUserId] = useState<string>("");

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
  const [clockLocations, setClockLocations] = useState<ClockLocation[]>([]);
  const handleProductsLoaded = useCallback(
    (data: Product[]) => setProducts(data),
    []
  );
  const handleClockLocationsLoaded = useCallback((data: ClockLocation[]) => setClockLocations(data), []);

  const { loading: getLoading, execute: fetchProducts } = useGet<Product>({
    route: "/products",
    autoFetch: false,
    onSuccess: handleProductsLoaded,
  });

  const { loading: clockLocationsLoading, execute: fetchClockLocations } = useGet<ClockLocation>({
    route: "/clock-locations",
    autoFetch: false,
    onSuccess: handleClockLocationsLoaded,
  });

  const [auction, setAuction] = useState<Auction>({
    auctionneerId: "",
    description: "",
    auctionDate: defaultAuctionDate(),
    auctionTime: null,
    productIds: [],
    clockLocationId: null,
  });

  /* ---------- Helpers ---------- */

  const handleChange = (
    key: keyof Auction,
    value: string | string[] | null
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
    if (!auction.auctionneerId) {
      alert("Please wait for user information to load.");
      return;
    }
    // Convert clockLocationId to null if empty string
    const payload = {
      ...auction,
      clockLocationId: auction.clockLocationId || null,
    };
    await createAuction(payload);
  };

  /* ---------- Effects ---------- */

  useEffect(() => {
    async function load() {
      try {
        // Fetch user info to get ID
        const userRes = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/info`, {
          credentials: "include",
        });
        if (userRes.ok) {
          const userData = await userRes.json();
          setUserId(userData.id);
          setAuction((prev) => ({ ...prev, auctionneerId: userData.id }));
        }
        await fetchProducts();
        await fetchClockLocations();
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
      name: "clockLocationId",
      label: "Clock Location (Optional)",
      type: "custom",
      colSpan: 2,
      render: ({ value, setValue }) => {
        return (
          <div>
            {clockLocationsLoading ? (
              <p className="text-gray-500">Loading clock locations...</p>
            ) : (
              <select
                value={value || ""}
                onChange={(e) => setValue("clockLocationId", e.target.value || null)}
                className="mt-1 w-full border rounded-lg p-2"
              >
                <option value="">None</option>
                {clockLocations.map((cl) => (
                  <option key={cl.id} value={cl.id}>
                    {cl.name}
                  </option>
                ))}
              </select>
            )}
          </div>
        );
      },
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
