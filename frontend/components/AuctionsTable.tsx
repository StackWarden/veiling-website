"use client";

type Auction = {
    id: string;
    description: string;
    startdate: string;
    enddate: string;
    status: string;
};

export default function AuctionTable({ auctions }: { auctions: Auction[] }) {
  return (
    <section className="bg-white rounded-xl shadow-md p-6 w-full max-w-5xl">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl font-semibold">Available Auctions</h2>

        <button
          className="p-1 hover:bg-gray-100 rounded-full"
          aria-label="Add auction"
        >
          <svg
            width="30"
            height="30"
            viewBox="0 0 41 41"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M22.5499 8.20002C22.5499 7.06612 21.6338 6.15002 20.4999 6.15002C19.366 6.15002 18.4499 7.06612 18.4499 8.20002V18.45H8.1999C7.066 18.45 6.1499 19.3661 6.1499 20.5C6.1499 21.6339 7.066 22.55 8.1999 22.55H18.4499V32.8C18.4499 33.9339 19.366 34.85 20.4999 34.85C21.6338 34.85 22.5499 33.9339 22.5499 32.8V22.55H32.7999C33.9338 22.55 34.8499 21.6339 34.8499 20.5C34.8499 19.3661 33.9338 18.45 32.7999 18.45H22.5499V8.20002Z"
              fill="black"
            />
          </svg>
        </button>
      </div>

      {auctions.length === 0 ? (
        <p className="text-gray-500">No auctions available.</p>
      ) : (
        <table className="w-full border-collapse border border-gray-300 text-left">
          <thead className="bg-gray-100">
            <tr>
              <th className="border border-gray-300 p-2">Description</th>
              <th className="border border-gray-300 p-2">Start Date</th>
              <th className="border border-gray-300 p-2">End Date</th>
              <th className="border border-gray-300 p-2">Status</th>
            </tr>
          </thead>
          <tbody>
            {auctions.map((a) => (
              <tr key={a.id} className="hover:bg-gray-50">
                <td className="border border-gray-300 p-2">{a.description}</td>
                <td className="border border-gray-300 p-2">{a.startdate ?? "-"}</td>
                <td className="border border-gray-300 p-2">{a.enddate ?? "-"}</td>
                <td className="border border-gray-300 p-2">{a.status ?? "-"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}