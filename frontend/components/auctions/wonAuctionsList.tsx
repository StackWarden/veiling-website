"use client";

import { useEffect, useState } from "react";
import useGet from "@/components/api/get";
import List, { ListHeader } from "@/components/list";

type Auction = {
  id: string;
  description: string;
  auctionDate: string;
  auctionTime: string | null;
  status: string;
  clockLocationName?: string | null;
};

export default function WonAuctionsDashboard() {
  const [auctions, setAuctions] = useState<Auction[]>([]);

  const { loading, execute: fetchAuctions } = useGet<Auction>({
    route: "/auctions/won",
    autoFetch: false,
    onSuccess: (data) => {
      const formatted = (Array.isArray(data) ? data : []).map((a) => {
        const dateLabel = a.auctionDate
          ? new Date(a.auctionDate).toLocaleDateString()
          : "-";

        const timeLabel = a.auctionTime ? a.auctionTime : "No time known";

        return {
          ...a,
          description: a.description || "-",
          auctionDate: dateLabel,
          auctionTime: timeLabel,
          status: a.status || "-",
          clock: a.clockLocationName || "-",
        };
      });

      setAuctions(formatted);
    },
  });

  useEffect(() => {
    fetchAuctions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const headers: ListHeader[] = [
    { key: "description", label: "Description", align: "start" },
    { key: "auctionDate", label: "Auction Date", align: "center" },
    { key: "auctionTime", label: "Time", align: "center" },
    { key: "status", label: "Status", align: "center" },
    { key: "clock", label: "Clock Location", align: "center" },
  ];

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        <div className="flex items-center w-full pt-8 pb-1">
          <div className="flex-1" />
          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Won Auctions
          </h1>
          <div className="flex-1" />
        </div>

        {loading && (
          <p className="text-gray-500 text-center py-6">Loading auctions...</p>
        )}

        {!loading && auctions.length === 0 && (
          <p className="text-gray-500 text-center py-6">
            You haven&apos;t won any auctions yet.
          </p>
        )}

        {!loading && auctions.length > 0 && (
          <List
            headers={headers}
            rows={auctions.map((auction) => ({
              ...auction,
            }))}
            onRowClick={(auction) => {
              window.location.href = `/auctions/auction/${auction.id}`;
            }}
            rowKey="id"
          />
        )}
      </div>
    </section>
  );
}
