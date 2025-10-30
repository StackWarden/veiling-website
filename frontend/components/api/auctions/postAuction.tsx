"use client";

import { useState } from "react";
import { usePostData } from "../post";
interface Auction {
  startTime: string;
  endTime: string;
}

export default function PostAuction() {
  const { loading, error, success, postData } = usePostData<Auction>("/auctions");

  const [auction, setAuction] = useState<Auction>({
    startTime: new Date().toISOString(),
    endTime: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // +7 days
  });

  const handleChange = (key: keyof Auction, value: string) => {
    setAuction((prev) => ({ ...prev, [key]: value }));
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await postData(auction);
  };

  return (
    <>
      <h1 className="text-2xl font-semibold text-center mb-6">Create Auction</h1>

      <form onSubmit={handleSubmit} className="flex flex-col space-y-4">
        <div className="flex flex-col">
          <label htmlFor="startTime" className="text-gray-700 mb-1">Start Time</label>
          <input
            type="datetime-local"
            id="startTime"
            value={auction.startTime.slice(0, 16)}
            onChange={(e) => handleChange("startTime", e.target.value)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="flex flex-col">
          <label htmlFor="endTime" className="text-gray-700 mb-1">End Time</label>
          <input
            type="datetime-local"
            id="endTime"
            value={auction.endTime.slice(0, 16)}
            onChange={(e) => handleChange("endTime", e.target.value)}
            className="border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="mt-4 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors"
        >
          {loading ? "Posting..." : "Create Auction"}
        </button>
      </form>

      {error && <p className="text-red-600 text-center mt-4">{error}</p>}
      {success && <p className="text-green-600 text-center mt-4">Auction created successfully!</p>}
      </>
  );
}