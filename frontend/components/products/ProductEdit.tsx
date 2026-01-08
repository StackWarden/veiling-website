"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useParams, useRouter } from "next/navigation";

type Species = {
  id: string;
  title: string;
};

type Product = {
  id: string;
  species: Species;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  photoUrl: string | null;
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
  });
  const [photo, setPhoto] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

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
      });
    };

    fetchProduct();
  }, [id]);

  /* ---------- Save ---------- */
  const saveChanges = async () => {
    let photoUrl: string | null = product?.photoUrl ?? null;

    // Upload new image to Vercel Blob if a photo is selected
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

        if (!response.ok) {
          throw new Error('Failed to upload image');
        }

        const blob = await response.json();
        photoUrl = blob.url;
      } catch (err) {
        console.error('Error uploading image:', err);
        alert('Failed to upload image. Please try again.');
        setUploading(false);
        return;
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
      }),
    });

    alert("Product updated!");
    router.push("/products");
  };

  if (!product) {
    return <p className="text-center mt-10">Loading...</p>;
  }

  /* ---------- UI ---------- */
  return (
    <div className="w-full flex flex-col items-center mt-12">
      <h1 className="text-[32px] font-bold text-[#162218] mb-10">
        Product info
      </h1>

      <div className="flex gap-16">
        {/* PHOTO */}
        <div className="rounded-xl overflow-hidden shadow-sm border border-[#D9D9D9] w-[350px] h-[350px] flex flex-col">
          {product.photoUrl ? (
            <Image
              src={product.photoUrl}
              alt={product.species.title}
              width={350}
              height={350}
              className="object-cover w-full h-full"
            />
          ) : (
            <div className="w-full h-full bg-gray-100 flex items-center justify-center text-gray-400">
              No Photo
            </div>
          )}
          <div className="p-2">
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

        {/* INFO */}
        <div className="border border-[#D9D9D9] rounded-xl p-8 w-[380px] shadow-sm">
          <h2 className="text-2xl font-semibold text-[#162218] mb-8">
            {product.species.title}
          </h2>

          {[
            { label: "Pot Size", key: "potSize" as FormKey },
            { label: "Stem Length (cm)", key: "stemLength" as FormKey },
            { label: "Quantity", key: "quantity" as FormKey },
            { label: "Price (€)", key: "minPrice" as FormKey },
          ].map(({ label, key }) => (
            <div
              key={key}
              className="flex justify-between items-center mb-5"
            >
              <span className="font-medium">{label}</span>

              {editField === key ? (
                <input
                  type="text"
                  value={form[key]}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, [key]: e.target.value }))
                  }
                  className="border border-gray-300 rounded px-3 py-1 text-sm w-32"
                />
              ) : (
                <span className="text-gray-700">{form[key]}</span>
              )}

              <button
                className="text-sm ml-4 text-[#162218] hover:underline underline-offset-2"
                onClick={() => setEditField(editField === key ? null : key)}
              >
                Edit
              </button>
            </div>
          ))}

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
  );
}
