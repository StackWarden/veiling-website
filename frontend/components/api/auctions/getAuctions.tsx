"use client";

import useGet from "../get";
import Navigation from "@/components/navigation";
import Link from "next/link";

interface Auction {
  id: string;
  auctionneerId: string;
  startTime: string;
  endTime: string;
  status: string;
}

export default function GetAuctions() {
  const { data, loading, error, refetch } = useGet<Auction>({ route: "/auctions" });

  return (    
    <div className="space-y-4">
      <Navigation title="John Doe (Member) - Auctions"/>
      <div className="flex items-center justify-between">
        <button
          onClick={() => void refetch()}
          disabled={loading}
          className="rounded border border-gray-300 bg-white px-3 py-1 text-sm font-medium transition hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {loading ? "Refreshing" : "Refresh"}
        </button>
        <Link
          className="p-1 hover:bg-gray-100 rounded-full"
          aria-label="Add auction"
          href="auctions/create"
        >
          <svg
            width="30"
            height="30"
            viewBox="0 0 41 41"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M22.5499 8.20002C22.5499 7.06612 21.6338 6.15002 20.4999 6.15002C19.366 6.15002 18.4499 7.06612 18.4499 8.20002V18.45H8.1999C7.066 18.45 6.1499 19.3661 6.1499 20.5C6.1499 21.6339 7.066 22.55 8.1999 22.55H18.4499V32.8C18.4499 33.9339 19.366 34.85 20.4999 34.85C21.6338 34.85 22.5499 33.9339 22.5499 32.8V22.55H32.7999C33.9338 22.55 34.8499 21.6339 34.8499 20.5C34.8499 19.3661 33.9338 18.45 32.7999 18.45H22.5499V8.20002Z"
              fill="black"
            />
          </svg>
        </Link>
      </div>

      {error && <p className="text-sm text-red-500">{error}</p>}

      <div className="max-h-[500px] overflow-auto">
        <table className="w-full border-collapse text-left">
          <thead className="border-b bg-gray-100 font-semibold">
            <tr>
              <th className="w-32 p-2">Status</th>
              <th className="w-48 p-2">Start time</th>
              <th className="w-48 p-2">End time</th>
            </tr>
          </thead>

          <tbody>
            {!loading &&
              data.map((auction) => (
                <tr className="border-b hover:bg-gray-50" key={auction.id}>
                  <td className="p-2">{auction.status}</td>
                  <td className="p-2">{new Date(auction.startTime).toLocaleString()}</td>
                  <td className="p-2">{new Date(auction.endTime).toLocaleString()}</td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
