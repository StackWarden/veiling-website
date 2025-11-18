"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
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


    <div className="w-full max-w-[90rem] px-4">

        <div className="flex items-center mb-6 w-full">
          {/* spacer div */}
          <div className="flex-1" />

          <h1 className="text-[32px] font-bold text-[#162218] text-center flex-1">
            Auction Schedule
          </h1>

          <Link href="auctions/create" className="flex-1 flex justify-end">
            <p 
              className="flex flex-row items-center gap-2 p-1 rounded-full hover:cursor-pointer"
              aria-label="Create auction"
            > 
              <span className="text-[#162218] font-medium">Create Auction</span>
              <Image
                src="/images/Plus.svg"
                alt="Create Auction Icon"
                width={40}
                height={40}
                priority
              />
            </p>
          </Link>
      </div>

      {loading ? (
        <p className="text-gray-500 text-center py-6">Loading auctions...</p>
      ) : auctions.length === 0 ? (
        <p className="text-gray-500 text-center py-6">No auctions available.</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-[D9D9D9] p-4">
          <table className="w-full border-collapse text-left">

            <thead className="bg-white">
              <tr className="text-[#4D4D4D]">
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
                    <td className="p-4 text-start rounded-l-2xl">{a.description ?? "-"}</td>
                    <td className="p-4 text-center">{a.startTime ? new Date(a.startTime).toLocaleString() : "-"}</td> 
                    <td className="p-4 text-center">{a.endTime ? new Date(a.endTime).toLocaleString() : "-"}</td> 
                    <td className="p-4 text-end">{a.status ?? "-"}</td>
                    <td className="p-4 text-right rounded-r-2xl">
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

