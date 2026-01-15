"use client";

import { useEffect, useState } from "react";
import useGet from "@/components/api/get";
import List, { ListHeader } from "@/components/list";

type WonAuctionItem = {
  auctionItemId: string;
  auctionId: string;
  auctionDescription: string;
  auctionDate: string;
  auctionTime: string | null;
  productId: string;
  productSpecies: string;
  soldAmount: number;
  soldPrice: number;
  pricePerUnit: number;
  soldAtUtc: string | null;
};

export default function WonAuctionsDashboard() {
  const [items, setItems] = useState<WonAuctionItem[]>([]);

  const { loading, execute: fetchItems } = useGet<WonAuctionItem>({
    route: "/auctions/won",
    autoFetch: false,
    onSuccess: (data) => {
      const formatted = (Array.isArray(data) ? data : []).map((item) => {
        const dateLabel = item.auctionDate
          ? new Date(item.auctionDate).toLocaleDateString()
          : "-";

        const timeLabel = item.auctionTime ? item.auctionTime : "-";

        const pricePerUnitFormatted = `€${item.pricePerUnit.toFixed(2)}`;
        const totalPriceFormatted = `€${item.soldPrice.toFixed(2)}`;

        return {
          ...item,
          auctionDescription: item.auctionDescription || "-",
          auctionDate: dateLabel,
          auctionTime: timeLabel,
          pricePerUnitFormatted,
          totalPriceFormatted,
        };
      });

      setItems(formatted);
    },
  });

  useEffect(() => {
    fetchItems();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const headers: ListHeader[] = [
    { key: "productSpecies", label: "Product", align: "start" },
    { key: "auctionDescription", label: "Auction", align: "start" },
    { key: "auctionDate", label: "Auction Date", align: "center" },
    { key: "auctionTime", label: "Time", align: "center" },
    { key: "soldAmount", label: "Amount", align: "center" },
    { key: "pricePerUnitFormatted", label: "Price per Unit", align: "end" },
    { key: "totalPriceFormatted", label: "Total Price", align: "end" },
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
          <p className="text-gray-500 text-center py-6">Loading won items...</p>
        )}

        {!loading && items.length === 0 && (
          <p className="text-gray-500 text-center py-6">
            You haven&apos;t won any auction items yet.
          </p>
        )}

        {!loading && items.length > 0 && (
          <List
            headers={headers}
            rows={items.map((item) => ({
              ...item,
            }))}
            onRowClick={(item) => {
              window.location.href = `/auctions/auction/${item.auctionId}`;
            }}
            rowKey="auctionItemId"
          />
        )}
      </div>
    </section>
  );
}
