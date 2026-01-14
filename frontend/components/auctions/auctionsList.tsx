"use client";

import { useEffect, useState } from "react";
import useDelete from "@/components/api/delete";
import useGet from "@/components/api/get";
import { RoleGate } from "@/components/RoleGate";
import List from "@/components/list";
import CreateButton from "@/components/createButton";

type Auction = {
  id: string;
  description: string;
  auctionDate: string; // YYYY-MM-DD
  auctionTime: string | null; // HH:mm or null
  status: string;
  clockLocationName?: string | null;
};

export default function AuctionsDashboard() {
  const [auctions, setAuctions] = useState<Auction[]>([]);

  const { loading: deleting, execute: deleteAuction } = useDelete({
    baseRoute: "/auctions",
    onSuccess: (id) => {
      setAuctions((prev) => prev.filter((a) => a.id !== id));
    },
  });

  const { loading, execute: fetchAuctions } = useGet<Auction>({
    route: "/auctions",
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

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure?")) return;
    await deleteAuction(id);
  };

  const handleSetTime = (id: string) => {
    window.location.href = `/auctions/auction/${id}/set-time`;
  };

  return (
    <section className="w-full flex flex-col items-center mt-12 px-4">
      <div className="w-full max-w-[90rem] px-4">
        <div className="flex items-center w-full pt-8 pb-1">
          <div className="flex-1" />
          <h1 className="text-[64px] font-bold text-[#162218] text-center flex-[3]">
            Auction Schedule
          </h1>

          <RoleGate
            allow={["auctioneer"]}
            fallback={<div className="flex-1 flex justify-end" />}
          >
            <CreateButton href="/auctions/create" label="Create Auction" />
          </RoleGate>
        </div>

        {(loading || deleting) && (
          <p className="text-gray-500 text-center py-6">Loading auctions...</p>
        )}

        {!loading && !deleting && auctions.length === 0 && (
          <p className="text-gray-500 text-center py-6">No auctions available.</p>
        )}

        {!loading && !deleting && auctions.length > 0 && (
          <List
            headers={[
              { key: "description", label: "Description", align: "start" },
              { key: "auctionDate", label: "Auction Date", align: "center" },
              { key: "auctionTime", label: "Time", align: "center" },
              { key: "status", label: "Status", align: "center" },
              { key: "clock", label: "Clock Location", align: "center" },
            ]}
            rows={auctions.map((auction) => ({
              ...auction,
              actions: (
                <RoleGate allow={["auctioneer"]}>
                  <div className="flex items-center justify-end gap-4">
                    <p
                      onClick={(e) => {
                        e.stopPropagation();
                        handleSetTime(auction.id);
                      }}
                      className="hover:cursor-pointer hover:underline underline-offset-2 hover:text-[#7fae8b]"
                    >
                      Set time
                    </p>

                    <p
                      onClick={(e) => {
                        e.stopPropagation();
                        handleDelete(auction.id);
                      }}
                      className="hover:cursor-pointer hover:underline underline-offset-2 text-red-600 hover:text-red-400"
                    >
                      Delete
                    </p>
                  </div>
                </RoleGate>
              ),
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
