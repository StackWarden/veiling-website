"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useParams, useRouter } from "next/navigation";
import useGet from "../api/get";

type Species = {
  id: string;
  title: string;
};

type ClockLocation = {
  id: string;
  name: string;
};

type Product = {
  id: string;
  species: Species;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  photoUrl: string | null;
  clockLocationId?: string | null;
  clockLocation?: ClockLocation | null;
};

export default function ProductEdit() {
  const { id } = useParams();
  const router = useRouter();

  const [product, setProduct] = useState<Product | null>(null);
  const [editField, setEditField] = useState<string | null>(null);

  const [form, setForm] = useState({
    potSize: "",
    stemLength: "",
    quantity: "",
    minPrice: "",
    clockLocationId: "",
  });
  const [photo, setPhoto] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  /* ---------- GET Clock Locations ---------- */
  const {
    data: clockLocations,
    loading: clockLocationsLoading,
  } = useGet<ClockLocation>({
    route: "/clock-locations",
  });

  type FormKey = keyof typeof form;

  /* ---------- Fetch ---------- */
  useEffect(() => {
    const fetchProduct = async () => {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/products/${id}`,
        { credentials: "include" }
      );

      const data: Product = await res.json();
      setProduct(data);

      setForm({
        potSize: data.potSize,
        stemLength: String(data.stemLength),
        quantity: String(data.quantity),
        minPrice: String(data.minPrice),
        clockLocationId: data.clockLocationId || "",
      });
    };

    fetchProduct();
  }, [id]);

  /* ---------- Save ---------- */
  const saveChanges = async () => {
    let photoUrl: string | null = product?.photoUrl ?? null;

    // Upload new image to Vercel Blob if a photo is selected
    // If upload fails (no token cause we broke ahh students)), keep existing image or continue without
    if (photo) {
      try {
        setUploading(true);
        const timestamp = Date.now();
        const filename = `products/${timestamp}-${photo.name}`;
        
        const response = await fetch(
          `/api/upload?filename=${encodeURIComponent(filename)}`,
          {
            method: 'POST',
            body: photo,
          }
        );

        if (response.ok) {
          const blob = await response.json();
          photoUrl = blob.url;
        } else {
          // If upload fails (no token cause we broke ahh students)), keep existing image
          console.warn('Image upload failed, keeping existing image');
          // photoUrl already set to product?.photoUrl ?? null above
        }
      } catch (err) {
        // If upload fails, keep existing image
        console.warn('Image upload failed, keeping existing image:', err);
        // photoUrl already set to product?.photoUrl ?? null above
      } finally {
        setUploading(false);
      }
    }

    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/products/${id}`, {
      method: "PUT",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        potSize: form.potSize,
        stemLength: Number(form.stemLength),
        quantity: Number(form.quantity),
        minPrice: Number(form.minPrice),
        photoUrl,
        clockLocationId: form.clockLocationId || null,
      }),
    });

    setSuccessMessage("Product updated successfully!");
    setTimeout(() => {
      router.push("/products");
    }, 1500);
  };

  if (!product) {
    return <p className="text-center mt-10">Loading...</p>;
  }

  /* ---------- UI ---------- */
  return (
    <div className="w-full flex flex-col items-center mt-12 px-4">
      <h1 className="text-[32px] font-bold text-[#162218] mb-10">
        Product info
      </h1>

      {successMessage && (
        <div className="mb-4 px-4 py-3 bg-green-100 border border-green-300 text-green-700 rounded-lg max-w-4xl w-full">
          {successMessage}
        </div>
      )}

      <div className="w-full max-w-6xl">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* COLUMN 1: PHOTO */}
          <div className="lg:col-span-1">
            <div className="rounded-xl overflow-hidden shadow-sm border border-[#D9D9D9] w-full aspect-square flex flex-col">
              {product.photoUrl ? (
                <Image
                  src={product.photoUrl}
                  alt={product.species.title}
                  width={400}
                  height={400}
                  className="object-cover w-full h-full"
                />
              ) : (
                <div className="w-full h-full bg-gray-100 flex items-center justify-center text-gray-400">
                  No Photo
                </div>
              )}
              <div className="p-3 border-t border-gray-200">
                <input
                  type="file"
                  accept="image/*"
                  onChange={(e) => setPhoto(e.target.files?.[0] || null)}
                  className="w-full text-sm"
                />
                {photo && (
                  <p className="text-xs text-gray-600 mt-1">
                    New image: {photo.name}
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* COLUMN 2 & 3: ALL DETAILS IN ONE BOX */}
          <div className="lg:col-span-2">
            <div className="border border-[#D9D9D9] rounded-xl p-6 shadow-sm">
              <h2 className="text-2xl font-semibold text-[#162218] mb-6 pb-4 border-b border-gray-200">
                {product.species.title}
              </h2>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* LEFT COLUMN */}
                <div className="space-y-4">
                  {[
                    { label: "Pot Size", key: "potSize" as FormKey, type: "text" },
                    { label: "Stem Length", key: "stemLength" as FormKey, type: "number", unit: "cm" },
                    { label: "Quantity", key: "quantity" as FormKey, type: "number" },
                  ].map(({ label, key, type, unit }) => (
                    <div
                      key={key}
                      className="grid grid-cols-3 gap-4 items-center"
                    >
                      <span className="font-medium text-gray-700 text-sm">{label}</span>
                      <div className="col-span-2">
                        {editField === key ? (
                          <div className="flex items-center gap-2">
                            <input
                              type={type}
                              value={form[key]}
                              onChange={(e) =>
                                setForm((f) => ({ ...f, [key]: e.target.value }))
                              }
                              onBlur={() => setEditField(null)}
                              onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                  setEditField(null);
                                }
                                if (e.key === "Escape") {
                                  setEditField(null);
                                  if (product) {
                                    setForm((f) => ({
                                      ...f,
                                      [key]: key === "potSize" 
                                        ? product.potSize 
                                        : key === "stemLength"
                                        ? String(product.stemLength)
                                        : String(product.quantity)
                                    }));
                                  }
                                }
                              }}
                              autoFocus
                              className="flex-1 border border-gray-300 rounded px-3 py-1.5 text-sm"
                            />
                            {unit && <span className="text-sm text-gray-500">{unit}</span>}
                          </div>
                        ) : (
                          <div className="flex items-center justify-between">
                            <span className="text-gray-700">
                              {form[key]}
                              {unit && <span className="text-gray-500 ml-1">{unit}</span>}
                            </span>
                            <button
                              className="text-xs text-[#162218] hover:underline underline-offset-2"
                              onClick={() => setEditField(editField === key ? null : key)}
                            >
                              {editField === key ? "Done" : "Edit"}
                            </button>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>

                {/* RIGHT COLUMN */}
                <div className="space-y-4">
                  {[
                    { label: "Price", key: "minPrice" as FormKey, type: "number", step: "0.01", unit: "€" },
                  ].map(({ label, key, type, step, unit }) => (
                    <div
                      key={key}
                      className="grid grid-cols-3 gap-4 items-center"
                    >
                      <span className="font-medium text-gray-700 text-sm">{label}</span>
                      <div className="col-span-2">
                        {editField === key ? (
                          <div className="flex items-center gap-2">
                            {unit && <span className="text-sm text-gray-500">{unit}</span>}
                            <input
                              type={type}
                              step={step}
                              value={form[key]}
                              onChange={(e) =>
                                setForm((f) => ({ ...f, [key]: e.target.value }))
                              }
                              onBlur={() => setEditField(null)}
                              onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                  setEditField(null);
                                }
                                if (e.key === "Escape") {
                                  setEditField(null);
                                  if (product) {
                                    setForm((f) => ({
                                      ...f,
                                      [key]: String(product.minPrice)
                                    }));
                                  }
                                }
                              }}
                              autoFocus
                              className="flex-1 border border-gray-300 rounded px-3 py-1.5 text-sm"
                            />
                          </div>
                        ) : (
                          <div className="flex items-center justify-between">
                            <span className="text-gray-700">
                              {unit && <span className="text-gray-500 mr-1">{unit}</span>}
                              {form[key]}
                            </span>
                            <button
                              className="text-xs text-[#162218] hover:underline underline-offset-2"
                              onClick={() => setEditField(editField === key ? null : key)}
                            >
                              {editField === key ? "Done" : "Edit"}
                            </button>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}

                  {/* Clock Location */}
                  <div className="grid grid-cols-3 gap-4 items-center">
                    <span className="font-medium text-gray-700 text-sm">Clock Location</span>
                    <div className="col-span-2">
                      {editField === "clockLocationId" ? (
                        <div className="flex items-center gap-2">
                          <select
                            value={form.clockLocationId}
                            onChange={(e) =>
                              setForm((f) => ({ ...f, clockLocationId: e.target.value }))
                            }
                            className="flex-1 border border-gray-300 rounded px-3 py-1.5 text-sm"
                            onBlur={() => setEditField(null)}
                            autoFocus
                          >
                            <option value="">None</option>
                            {clockLocationsLoading ? (
                              <option>Loading...</option>
                            ) : (
                              clockLocations?.map((cl) => (
                                <option key={cl.id} value={cl.id}>
                                  {cl.name}
                                </option>
                              ))
                            )}
                          </select>
                          <button
                            className="text-xs text-[#162218] hover:underline underline-offset-2 whitespace-nowrap"
                            onClick={() => setEditField(null)}
                          >
                            Done
                          </button>
                        </div>
                      ) : (
                        <div className="flex items-center justify-between">
                          <span className="text-gray-700">
                            {product.clockLocation?.name || "None"}
                          </span>
                          <button
                            className="text-xs text-[#162218] hover:underline underline-offset-2"
                            onClick={() => setEditField("clockLocationId")}
                          >
                            Edit
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              <button
                onClick={saveChanges}
                disabled={uploading}
                className="mt-8 w-full bg-[#162218] text-white py-3 rounded-lg hover:bg-[#0f1c14] transition disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {uploading ? 'Uploading image...' : 'Save changes'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
