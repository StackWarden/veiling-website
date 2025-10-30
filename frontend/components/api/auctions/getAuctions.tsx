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
    <div className="max-h-[500px] overflow-auto">
      <table className="w-full text-left border-collapse">
        <thead className="bg-gray-100 font-semibold border-b">
          <tr>
            <th className="p-2 w-32">Status</th>
            <th className="p-2 w-48">Start time</th>
            <th className="p-2 w-48">End time</th>
          </tr>
        </thead>

        <tbody>
          <GetApi<Auction>
            route="/auctions"
            title="Auctions"
            renderItem={(auction) => (
              <tr className="border-b hover:bg-gray-50" key={auction.id}>
                <td className="p-2">{auction.status}</td>
                <td className="p-2">{new Date(auction.startTime).toLocaleString()}</td>
                <td className="p-2">{new Date(auction.endTime).toLocaleString()}</td>
              </tr>
            )}
          />
        </tbody>
      </table>
    </div>
  );
}
