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

  <div className="w-full h-full bg-white ">

          <h1 className="text-xl font-semibold text-center mb-1">Auction Schedule</h1>

          <div className="flex justify-center overflow-x-auto">
            {loading ? (
              <p className="p-4 text-gray-500">Loading...</p>
          ) : ( 
                  <AuctionTable initialAuctions={auctions} />
          )}
      </div>
  </div>
    );
  }
