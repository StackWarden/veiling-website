"use client";

import PostApi from "../post";

interface Auction {
  startTime: string;
  endTime: string;
  status: string;
}

export default function PostAuction() {
  return (
    <PostApi<Auction>
      route="/auctions"
      title="Auction"
      initialData={{
        startTime: new Date().toISOString(),
        endTime: new Date(new Date().getTime() + 7 * 24 * 60 * 60 * 1000).toISOString(), // +7 days
        status: "",
      }}
    />
  );
}