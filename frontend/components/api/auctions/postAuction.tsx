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
      <div className="flex justify-center items-start px-6 pt-8">
        <form
          onSubmit={handleSubmit}
          className="w-full max-w-3xl space-y-8"
        >
          <h1 className="text-3xl font-bold text-center">Create Auction</h1>
          
          {/* Grid layout */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">

            {/* Start Time */}
            <div className="flex flex-col">
              <label className="font-semibold">Start Time</label>
              <input
                type="datetime-local"
                value={auction.startTime.slice(0, 16)}
                onChange={(e) => handleChange("startTime", e.target.value)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
              />
            </div>

            {/* End Time */}
            <div className="flex flex-col">
              <label className="font-semibold">End Time</label>
              <input
                type="datetime-local"
                value={auction.endTime.slice(0, 16)}
                onChange={(e) => handleChange("endTime", e.target.value)}
                className="mt-1 w-full border border-gray-300 rounded-lg p-2"
              />
            </div>

          </div>

          {/* Button */}
          <div className="flex justify-center">
            <button
              type="submit"
              disabled={loading}
              className="w-40 bg-[#162218] text-white py-3 rounded-lg font-semibold hover:bg-[#0f1c14] transition"
            >
              {loading ? "Posting..." : "Create Auction"}
            </button>
          </div>

          {/* Status messages */}
          {error && (
            <p className="text-red-600 text-center font-semibold">{error}</p>
          )}
          {success && (
            <p className="text-green-600 text-center font-semibold">
              Auction created successfully!
            </p>
          )}

        </form>
      </div>
    </>
  );
}
