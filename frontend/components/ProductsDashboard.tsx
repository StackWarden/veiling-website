"use client";

import React, { useEffect, useState } from "react";
import ProductTable from "@/components/ProductTable";

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

export default function ProductsDashboard() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);


  const fetchProducts = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/products`);
      const data = await res.json();
      setProducts(data);
    } catch (err) {
      console.error("Error fetching products:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  return (
<div className="w-full h-full bg-white rounded-2xl shadow-md p-8 flex flex-col">

        <h1 className="text-lg font-semibold text-center mb-8">Products</h1>

        <div className="flex-1 overflow-x-auto border border-gray-300 rounded">
          {loading ? (
            <p className="p-4 text-gray-500">Loading...</p>
        ) : ( 
                <ProductTable products={products} />
        )}
    </div>
</div>
  );
}
