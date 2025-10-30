"use client";

import React, { useState } from "react";

interface DataFetcherProps<T> {
  route: string; // dit is de route zoals /auctions of /products (alleen voor get)
  title?: string;
  renderItem: (item: T) => React.ReactNode;
}

export default function GetApi<T>({ route, title, renderItem }: DataFetcherProps<T>) {
  const [data, setData] = useState<T[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const fetchData = async () => {
    setLoading(true);
    setError("");

    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${route}`);
      if (!response.ok) throw new Error(`Failed to fetch ${route}`);
      const result = await response.json();
      setData(result);
    } catch (err) {
      if (err instanceof Error) setError(err.message);
      else setError("An unknown error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-2">
      {loading && <p>Loading...</p>}
      {error && <p className="text-red-500">{error}</p>}

      <ul>
        {data.map((item, i) => (
          <li key={i}>{renderItem(item)}</li>
        ))}
      </ul>

      <button onClick={fetchData}>Fetch {title ?? "Data"}</button>
    </div>
  );
}
