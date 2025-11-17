"use client";

import { useEffect, useState } from "react";
import useGet from "./api/get";
import Link from "next/link";
import { useRouter } from "next/navigation";


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
  const { data, error } = useGet<Auction>({ route: "/auctions" });
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const router = useRouter();

  const fetchAuctions = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/Auctions`);
      const data = await res.json();
      setAuctions(data);
    } catch (err) {
      console.error("Error fetching auctions:", err);
    } finally {
      setLoading(false);
    }
  };

  
 useEffect(() => {
   if (data && data.length) setAuctions(data);
  }, [data]);

  if (loading) {
    return <div>Loading...</div>;
  }

    const handleRowSelect = (id: string) => {
    if (!id) return;
    setSelectedId((prev) => (prev === id ? null : id));
  }

  const handleRowOpen = (id?: string) => {
    if (!id) return;
    router.push(`/auctions/${id}`);
  };

  const handleEdit = (id?: string) => {
    if (!id) return;
    router.push(`/auctions/${id}/edit`);
  };

  return (

  <div className="w-full h-full bg-white ">

          <h1 className="text-[64px] font-semibold text-center mb-1">Auction Schedule</h1>     

          <div className="flex justify-center overflow-x-auto">
            {loading ? (
              <p className="p-4 text-gray-500">Loading...</p>
          ) : ( 
                  <section className="bg-white rounded-xl shadow-md p-6 w-full max-w-5xl">
      <div className="flex justify-end items-center mb-4">
                   
        <div className="flex justify-end">
          <Link href ="auctions/create">
            <button 
              className="flex flex-row items-center gap-2 p-1 hover:bg-gray-300 rounded-full"
              aria-label="Create auction"
            > 
            <span className="text-black font-medium">Create Auction</span>

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
      </div>
      {auctions.length === 0 ? (
        <p className="text-gray-500">No auctions available.</p>
      ) : (
      <div className="rounded-lg overflow-hidden border border-black">
        <table className="w-full text-center">
          <thead>
            <tr>
              <th className="bg-white text-black p-2"></th>
              <th className="bg-white text-black p-2">Description</th>
              <th className="bg-white text-black p-2">Start Date</th>
              <th className="bg-white text-black p-2">End Date</th>
              <th className="bg-white text-black p-2">Status</th>
              <th className="bg-white text-black p-2"></th>
            </tr>
          </thead>
          <tbody>
            {auctions.map((a) => {
              const isSelected = selectedId === a.id;
              const trClass = `cursor-pointer ${isSelected ? "bg-black text-white" : "hover:bg-gray-100"} rounded-full px-2 py-1`;
              return (
              <tr key={a.id} 
                role="button"
                tabIndex={0}
                aria-pressed={isSelected}
                aria-label={a.description ? `Select auction: ${a.description}` : `Select auction ${a.id}`}
                onClick={() => handleRowSelect(a.id)}
                onDoubleClick={() => handleRowOpen(a.id)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    handleRowSelect(a.id);
                  }
                }}
              className={trClass} >
                <td className="bg-white text-black p-2"></td>
                <td className="rounded-l p-2">{a.description ?? "-"}</td>
                <td className="p-2">{a.startTime ? new Date(a.startTime).toLocaleString() : "-"}</td> 
                <td className="p-2">{a.endTime ? new Date(a.endTime).toLocaleString() : "-"}</td> 
                <td className="rounded-r p-2">{a.status ?? "-"}</td>
                <td className="bg-white text-black p-2">
                  {isSelected && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      handleEdit(a.id)
                    }}
                    aria-label={a.description ? `Edit ${a.description}` : `Edit auction ${a.id}`}
                  >
                    <img src="/edit.png" alt="Edit" className="w-5 h-5 inline-block" />
                  </button>
                  )}
                </td>
              </tr>
              )
            })}
          </tbody>
        </table>
      </div>
      )}
              </section>
            )}
          </div>
      </div>
  );
}