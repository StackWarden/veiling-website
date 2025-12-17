"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useParams } from "next/navigation";

/* ---------- Types ---------- */

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

/* ---------- Component ---------- */

export default function ProductInfo() {
  const { id } = useParams();
  const [product, setProduct] = useState<Product | null>(null);

  useEffect(() => {
    const fetchProduct = async () => {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/products/${id}`,
        { credentials: "include" }
      );

      const data: Product = await res.json();
      setProduct(data);
    };

    fetchProduct();
  }, [id]);

  if (!product) {
    return <p className="text-center mt-10">Loading...</p>;
  }

  return (
    <div className="w-full flex flex-col items-center mt-12">
      <h1 className="text-[32px] font-bold text-[#162218] mb-8">
        Product info
      </h1>

      <div className="flex gap-16">
        {/* PHOTO */}
        <div className="rounded-xl overflow-hidden shadow-sm border border-[#D9D9D9] w-[350px] h-[350px]">
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
        </div>

        {/* INFO */}
        <div className="border border-[#D9D9D9] rounded-xl p-8 w-[350px] shadow-sm">
          <h2 className="text-2xl font-semibold text-[#162218] mb-6">
            {product.species.title}
          </h2>

          {[
            { label: "Species", value: product.species.title },
            { label: "Pot Size", value: product.potSize },
            { label: "Stem Length (cm)", value: product.stemLength },
            { label: "Quantity", value: product.quantity },
            {
              label: "Price (€)",
              value: `€${product.minPrice.toFixed(2)}`,
            },
          ].map(({ label, value }) => (
            <div
              key={label}
              className="flex justify-between items-center mb-4"
            >
              <span className="font-medium text-black">{label}</span>
              <span className="text-gray-700">{value}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
