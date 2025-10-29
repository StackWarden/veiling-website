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
  const [menuOpen, setMenuOpen] = useState(false);
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
    
    <div className="relative w-screen min-h-screen flex bg-gray-100 overflow-hidden">

      
      <aside
        className={`fixed top-0 left-0 w-64 h-full bg-white border-r border-gray-300 p-6 flex flex-col justify-between shadow-lg transform transition-transform duration-300
          ${menuOpen ? "translate-x-0" : "-translate-x-full"}`}
      >
        <div>
          <nav className="flex flex-col space-y-4 text-lg font-semibold">
            <button className="text-left hover:text-blue-600">Auctions</button>
            <button className="text-left hover:text-blue-600">Messages</button>
          </nav>
        </div>
        <button className="text-left font-semibold hover:text-red-600">
          Logout
        </button>
      </aside>

      <main
        className={`flex-1 p-8 overflow-auto transition-all duration-300 ${
          menuOpen ? "ml-64" : "ml-0"
        }`}
      >
        <header className="w-full flex justify-between items-center mb-8">
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="p-2 rounded hover:bg-gray-200"
            aria-label="Toggle menu"
          >
            <svg
              width="28"
              height="22"
              viewBox="0 0 28 22"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M0 20.5H28M0 1.5H28M0 11H28"
                stroke="black"
                strokeWidth="3"
              />
            </svg>
          </button>

          <h1 className="text-2xl font-bold text-center flex-1">Products</h1>

          <div className="w-10" />
        </header>

        <div className="w-full overflow-x-auto">
          {loading ? <p>Loading...</p> : <ProductTable products={products} />}
        </div>
      </main>
    </div>
  );
}
