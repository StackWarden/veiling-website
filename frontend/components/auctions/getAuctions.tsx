"use client";
// Deprecated
import useGet from "../api/get";

interface Auction {
  id: string;
  auctionneerId: string;
  startTime: string;
  endTime: string;
  status: string;
}

export default function GetAuctions() {
  const { data, loading, error, execute } = useGet<Auction>({ route: "/auctions" });

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl">username, &apos;Role&apos;</h1>
      </div>
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Auctions</h2>
        <button
          onClick={() => void execute()}
          disabled={loading}
          className="rounded border border-gray-300 bg-white px-3 py-1 text-sm font-medium transition hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {loading ? "Refreshing" : "Refresh"}
        </button>
      </div>

      {error && <p className="text-sm text-red-500">{error}</p>}

      <div className="max-h-[500px] overflow-auto">
        <table className="w-full border-collapse text-left">
          <thead className="border-b bg-gray-100 font-semibold">
            <tr>
              <th className="w-32 p-2">Status</th>
              <th className="w-48 p-2">Start time</th>
              <th className="w-48 p-2">End time</th>
            </tr>
          </thead>

          <tbody>
            {!loading &&
              data.map((auction) => (
                <tr className="border-b hover:bg-gray-50" key={auction.id}>
                  <td className="p-2">{auction.status}</td>
                  <td className="p-2">{new Date(auction.startTime).toLocaleString()}</td>
                  <td className="p-2">{new Date(auction.endTime).toLocaleString()}</td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
