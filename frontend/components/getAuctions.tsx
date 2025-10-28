"use client";

import React, { useState } from "react";

interface Auction {
  id: string;
  auctionneerId: string;
  startTime: string;
  endTime: string;
  status: string;
}

export default function AuctionList() {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const fetchAuctions = async () => {
    setLoading(true);
    setError("");
    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auctions`);
      if (!response.ok) throw new Error("Failed to fetch auctions");
      const data: Auction[] = await response.json();
      setAuctions(data);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: "2rem" }}>
      <h1>Auctions</h1>
      <button onClick={fetchAuctions}>Get Auctions</button>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      <ul>
        {auctions.map((auction) => (
          <li key={auction.id}>
            <strong>{auction.status}</strong> — 
            Start: {new Date(auction.startTime).toLocaleString()} — 
            End: {new Date(auction.endTime).toLocaleString()}
          </li>
        ))}
      </ul>
    </div>
  );
};
