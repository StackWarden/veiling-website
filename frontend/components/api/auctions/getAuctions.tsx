"use client";

import GetApi from "../get";

interface Auction {
  id: string;
  auctionneerId: string;
  startTime: string;
  endTime: string;
  status: string;
}

export default function GetAuctions() {
  return (
    <GetApi<Auction>
      route="/auctions"
      title="Auctions"
      renderItem={(auction) => (
        <>
          <strong>{auction.status}</strong> — 
          Start: {new Date(auction.startTime).toLocaleString()} — 
          End: {new Date(auction.endTime).toLocaleString()}
        </>
      )}
    />
  );
}
