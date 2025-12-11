"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";

type Auction = {
  id: string;
  auctionneerId?: string;
  description?: string;
  startTime?: string;
  endTime?: string;
  status?: string;
};

const formatDate = (value?: string) =>
  value ? new Date(value).toLocaleString() : "—";

export default function AuctionsInfo() {
  const { id } = useParams<{ id?: string }>();
  const [auction, setAuction] = useState<Auction | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;

    const fetchAuction = async () => {
      setLoading(true);
      setError(null);

      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/auctions/${id}`,
          { credentials: "include" }
        );

        if (!res.ok) {
          throw new Error("Failed to fetch auction details.");
        }

        const data = await res.json();
        setAuction(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Unable to load auction.");
      } finally {
        setLoading(false);
      }
    };

    fetchAuction();
  }, [id]);

  if (!id) {
    return (
      <p className="text-center mt-10">
        Select an auction from the list to view its details.
      </p>
    );
  }

  if (loading) return <p className="text-center mt-10">Loading auction...</p>;
  if (error) return <p className="text-center mt-10 text-red-600">{error}</p>;
  if (!auction) return null;

  const detailRows = [
    { label: "Description", value: auction.description || "—" },
    { label: "Status", value: auction.status || "—" },
    { label: "Start Time", value: formatDate(auction.startTime) },
    { label: "End Time", value: formatDate(auction.endTime) },
    { label: "Auctioneer ID", value: auction.auctionneerId || "—" },
    { label: "Auction ID", value: auction.id },
  ];

  return (
    <div className="w-full flex flex-col items-center mt-12">
      <h1 className="text-[32px] font-bold text-[#162218] mb-8">
        Auction info
      </h1>

      <div className="border border-[#D9D9D9] rounded-xl p-8 w-fit shadow-sm">
        <h2 className="text-2xl font-semibold text-[#162218] mb-6">
          {auction.description || "Auction"}
        </h2>

        {detailRows.map(({ label, value }) => (
          <div key={label} className="flex justify-between items-center mb-4">
            <span className="font-medium text-black">{label}</span>
            <span className="text-gray-700 text-right">{value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
