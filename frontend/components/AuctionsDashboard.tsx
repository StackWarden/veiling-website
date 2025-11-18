"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

type Auction = {
    id: string;
    description?: string;
    startTime?: string;
    endTime?: string;
    status?: string;
};

export default function AuctionsDashboard() {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchAuctions = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auctions`);
      const data = await res.json();
      setAuctions(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("Error fetching auctions:", err);
    } finally {
      setLoading(false);
    }
  };

  
 useEffect(() => {
   fetchAuctions();
  }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this auction?")) return;

    try {
      await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auctions/${id}`, {
        method: "DELETE",
      });

      setAuctions((prev) => prev.filter((a) => a.id !== id));

    } catch (err) {
      console.error("Failed to delete product:", err);
    }
  };

  return (

  <section className="w-full flex flex-col items-center mt-12 px-4">

    <h1 className="text-[32px] font-bold mb-0 text-[#162218]">Auction Schedule</h1>     

      <div className="w-full max-w-[90rem] px-4">

        <div className="flex justify-end mb-6">
          <Link href ="auctions/create">
            <button 
              className="flex flex-row items-center gap-2 p-1 hover:bg-gray-300 rounded-full"
              aria-label="Create auction"
            > 
            <span className="text-[#162218] font-medium">Create Auction</span>

              <svg 
              width="30" 
              height="30" 
              viewBox="0 0 41 41" 
              fill="none" 
              xmlns="http://www.w3.org/2000/svg">
                <path d="M22.5499 8.20002C22.5499 7.06612 21.6338 6.15002 20.4999 6.15002C19.366 6.15002 18.4499 7.06612 18.4499 8.20002V18.45H8.1999C7.066 18.45 6.1499 19.3661 6.1499 20.5C6.1499 21.6339 7.066 22.55 8.1999 22.55H18.4499V32.8C18.4499 33.9339 19.366 34.85 20.4999 34.85C21.6338 34.85 22.5499 33.9339 22.5499 32.8V22.55H32.7999C33.9338 22.55 34.8499 21.6339 34.8499 20.5C34.8499 19.3661 33.9338 18.45 32.7999 18.45H22.5499V8.20002Z" fill="black"/>
              </svg>
            </button>
         </Link>
        </div>

      {loading ? (
        <p className="text-gray-500 text-center py-6">Loading auctions...</p>
      ) : auctions.length === 0 ? (
        <p className="text-gray-500 text-center py-6">No auctions available.</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-[D9D9D9]">
          <table className="w-full border-collapse text-left">

          <thead className="bg-white">
            <tr className="border-b border-[D9D9D9] text-[#4D4D4D]">

              <th className="p-3 text-start">Description</th>
              <th className="p-3 text-center">Start Date</th>
              <th className="p-3 text-center">End Date</th>
              <th className="p-3 text-end">Status</th>
            </tr>
          </thead>

          <tbody className="bg-white text-[1A1A1A]">

            {auctions.map((a) => {
              return (
              <tr key={a.id} className="hover:bg-[#162218] hover:text-white transition cursor-pointer">
                <td className="p-4 text-start">{a.description ?? "-"}</td>
                <td className="p-4 text-center">{a.startTime ? new Date(a.startTime).toLocaleString() : "-"}</td> 
                <td className="p-4 text-center">{a.endTime ? new Date(a.endTime).toLocaleString() : "-"}</td> 
                <td className="p-4 text-end">{a.status ?? "-"}</td>
                <td className="p-4 text-right">
                    <div className="flex gap-6 justify-end">
                      <Link
                        href={`/auctions/info/${a.id}`}
                        className="hover:underline underline-offset-2"
                      >
                        Edit 
                      </Link>
                      <button
                        onClick={() => handleDelete(a.id)}
                        className="hover:underline underline-offset-2 text-red-600 hover:text-red-400"
                        type="button"
                      >
                        Delete 
                      </button>
                    </div> 
                  </td>
              </tr>
              )
            })}
          </tbody>
        </table>
        </div>
      )}

      </div>
  </section>
  );
}

