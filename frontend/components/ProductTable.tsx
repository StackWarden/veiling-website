"use client";

type Product = {
  id: string;
  supplierId: string;
  species: string;
  potSize: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  clockLocation: string | number;
  auctionDate?: string | null;
  photoUrl?: string | null;
};

const getClockLocationName = (value: string | number) => {
  const mapping: Record<number, string> = {
    0: "Naaldwijk",
    1: "Aalsmeer",
    2: "Rijnsburg",
    3: "Eelde",
  };
  if (typeof value === "string" && isNaN(Number(value))) return value;
  return mapping[Number(value)] ?? "Onbekend";
};

export default function ProductTable({ products }: { products: Product[] }) {
  return (
    <section className="bg-white rounded-xl shadow-md p-6 w-full max-w-5xl">
      <h2 className="text-xl font-semibold mb-4">Available Products</h2>

      {products.length === 0 ? (
        <p className="text-gray-500">No products available.</p>
      ) : (
        <table className="w-full border-collapse border border-gray-300 text-left">
          <thead className="bg-gray-100">
            <tr>
              <th className="border border-gray-300 p-2">Species</th>
              <th className="border border-gray-300 p-2">Pot Size</th>
              <th className="border border-gray-300 p-2">Stem Length (cm)</th>
              <th className="border border-gray-300 p-2">Quantity</th>
              <th className="border border-gray-300 p-2">Price (€)</th>
              <th className="border border-gray-300 p-2">Location</th>
              <th className="border border-gray-300 p-2">Auction Date</th>
              <th className="border border-gray-300 p-2">Photo</th>
            </tr>
          </thead>
          <tbody>
            {products.map((p) => (
              <tr key={p.id} className="hover:bg-gray-50">
                <td className="border border-gray-300 p-2">{p.species}</td>
                <td className="border border-gray-300 p-2">{p.potSize}</td>
                <td className="border border-gray-300 p-2">{p.stemLength}</td>
                <td className="border border-gray-300 p-2">{p.quantity}</td>
                <td className="border border-gray-300 p-2">
                  €{p.minPrice.toFixed(2)}
                </td>
                <td className="border border-gray-300 p-2">
                  {getClockLocationName(p.clockLocation)}
                </td>
                <td className="border border-gray-300 p-2">
                  {p.auctionDate ?? "-"}
                </td>
                <td className="border border-gray-300 p-2 text-center">
                  {p.photoUrl ? (
                    <img
                      src={p.photoUrl}
                      alt={p.species}
                      className="w-12 h-12 object-cover rounded mx-auto"
                    />
                  ) : (
                    <span className="text-gray-400">No photo</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
