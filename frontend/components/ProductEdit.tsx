"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useParams, useRouter } from "next/navigation";

type Product = {
  id: string;
  species: string;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  clockLocation: string;
  auctionDate: string | null;
  photoUrl: string | null;
};

const Clock_locations = ["Naaldwijk", "Aalsmeer", "Rijnsburg", "Eelde"]

export default function ProductInfo() {
  const { id } = useParams();
  const router = useRouter();

  const [product, setProduct] = useState<Product | null>(null);
  const [editField, setEditField] = useState<string | null>(null);

  const [form, setForm] = useState({
    species: "",
    potSize: "",
    stemLength: "",
    quantity: "",
    minPrice: "",
    clockLocation: "",
    auctionDate: "",
  });

  type FormKey = keyof typeof form;


  useEffect(() => {
    const fetchProduct = async () => {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/products/${id}`);
      const data = await res.json();

      setProduct(data);

      setForm({
        species: data.species,
        potSize: data.potSize,
        stemLength: data.stemLength,
        quantity: data.quantity,
        minPrice: data.minPrice,
        clockLocation: Clock_locations[Number(data.clockLocation)],
        auctionDate: data.auctionDate ?? "",
      });
    };

    fetchProduct();
  }, [id]);

  const saveChanges = async () => {
    await fetch(`${process.env.NEXT_PUBLIC_API_URL}/products/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
      species: form.species,
      potSize: form.potSize,
      stemLength: Number(form.stemLength),
      quantity: Number(form.quantity),
      minPrice: Number(form.minPrice),
      clockLocation: form.clockLocation,      
      auctionDate: form.auctionDate || "",   
      photoUrl: product?.photoUrl ?? ""
      }),
    });

    alert("Product updated!");
    router.push("/products");
  };

  if (!product) return <p className="text-center mt-10">Loading...</p>;

  return (
    <div className="w-full flex flex-col items-center mt-12">
      <h1 className="text-[32px] font-bold text-[#162218] mb-8">Product info</h1>

      <div className="flex gap-16">
        {/* foto */}
        <div className="rounded-xl overflow-hidden shadow-sm border border-[#D9D9D9] w-[350px] h-[350px]">
          {product.photoUrl ? (
            <Image
              src={product.photoUrl}
              alt={product.species}
              width={350}
              height={350}
              className="object-cover w-full h-full"
            />
          ) : (
            <div className="w-full h-full bg-gray-100 flex items-center justify-center text-gray-400">
              No Photo
            </div>
          )}
        </div>

        {/* info */}
        <div className="border border-[#D9D9D9] rounded-xl p-8 w-[350px] shadow-sm">
          <h2 className="text-2xl font-semibold text-[#162218] mb-6">
            {product.species}
          </h2>

          {[
            { label: "Species", key: "species" as FormKey },
            { label: "Pot Size", key: "potSize" as FormKey },
            { label: "Stem Length (cm)", key: "stemLength" as FormKey },
            { label: "Quantity", key: "quantity" as FormKey},
            { label: "Price (€)", key: "minPrice" as FormKey },
            { label: "Clock location", key: "clockLocation" as FormKey },
            { label: "Auction date", key: "auctionDate" as FormKey},
          ].map(({ label, key }) => (
            <div key={key} className="flex justify-between items-center mb-4">
              <span className="font-medium text-black">{label}</span>

              {editField === key ? (
                key === "clockLocation" ? (
                    <select
                        value={form.clockLocation}
                        onChange={(e) =>
                      setForm((f) => ({ ...f, clockLocation: e.target.value }))
                    }
                    className="border border-gray-300 rounded px-2 py-1 text-sm w-32"
                  >
                    {Clock_locations.map((loc) => (
                      <option key={loc} value={loc}>
                        {loc}
                      </option>
                    ))}
                    </select>
                ) : (
                
                <input
                  type="text"
                  value={form[key]}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, [key]: e.target.value }))
                  }
                  className="border border-gray-300 rounded px-2 py-1 text-sm w-32"
                />
                )
              ) : (
                <span className="text-gray-700">
                  {form[key] || "—"}
                </span>
              )}

              <button
                className="text-sm ml-3 text-[#162218] hover:underline underline-offset-2"
                onClick={() => setEditField(editField === key ? null : key)}
              >
                Edit
              </button>
            </div>
          ))}

          <button
            onClick={saveChanges}
            className="mt-6 w-full bg-[#162218] text-white py-2 rounded-lg text-center hover:bg-[#0f1c14] transition"
          >
            Save changes
          </button>
        </div>
      </div>
    </div>
  );
}
