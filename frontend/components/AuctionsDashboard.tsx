"use client";

import React, { useEffect, useState } from "react";
import AuctionTable from "./AuctionsTable";

type Auction = {
  id: string;
  description: string;
  startdate: string;
  enddate: string;
  status: string;
};

export default function AuctionsDashboard() {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [loading, setLoading] = useState(false);

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
     fetchAuctions();
   }, []);

  if (loading) {
    return <div>Loading...</div>;
  }

    return (
  <div className="w-full h-full bg-white rounded-2xl shadow-md p-8 flex flex-col">

          <h1 className="text-lg font-semibold text-center mb-8">Auctions</h1>

          <div className="flex-1 overflow-x-auto border border-gray-300 rounded">
            {loading ? (
              <p className="p-4 text-gray-500">Loading...</p>
          ) : ( 
                  <AuctionTable auctions={auctions} />
          )}
      </div>
  </div>
    );
  }
