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
  // Container for the Auctions Dashboard
  <div className="w-full h-full bg-white">
    <header className="flex items-center justify-between p-8">
      <div className="flex items-center">
        <img src="/logo.png" alt="Logo" className="h-10 w-auto" />
      </div>
      <div className="Topnav-right">
          <a className = "active" href="auctions">Auctions</a>
          <a href="messages"> Messages</a>
          <a href="profile"> Profile</a>
      </div>
    </header>

          <h1 className="text-xl font-semibold text-center mb-1">Auctions</h1>

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
