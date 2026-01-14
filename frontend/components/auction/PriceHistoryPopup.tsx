"use client";

import { useEffect, useState } from "react";
import useGet from "@/components/api/get";
import { RoleGate } from "@/components/RoleGate";
import List from "@/components/list";
import CreateButton from "@/components/createButton";


type PriceHistoryEntry = {
  date: string;
  price: number;
};

type Props = {
  onClose?: () => void;
  productId?: string | null;
};

export default function PriceHistoryPopup({ onClose, productId }: Props) {
  const [entries, setEntries] = useState<PriceHistoryEntry[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let mounted = true;

    async function fetchHistory() {
      setLoading(true);
      try {
        const qs = productId ? `?productId=${encodeURIComponent(productId)}` : "";
        const res = await fetch(`/api/PriceHistory${qs}`);
        if (!mounted) return;
        if (res.ok) {
          const data = await res.json();
          // prefer currentSupplier.last10 if productId provided, otherwise allSuppliers.last10
          const pick = productId ? data.currentSupplier : data.allSuppliers;
          const list = pick?.last10 ?? [];
          setEntries(list.map((l: any) => ({ date: l.date, price: Number(l.price) })) ?? []);
        } else {
          setEntries([]);
        }
      } catch (e) {
        setEntries([]);
      } finally {
        if (mounted) setLoading(false);
      }
    }

    fetchHistory();

    return () => {
      mounted = false;
    };
  }, []);

  return (
    <div className="rounded-lg bg-white p-6 shadow-lg">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">Price history</h3>
        <button
          type="button"
          onClick={() => onClose && onClose()}
          className="rounded-md bg-neutral-100 px-3 py-1 text-sm"
        >
          Close
        </button>
      </div>

      <div className="mt-4 max-h-72 overflow-auto">
        {loading ? (
          <div className="text-sm text-neutral-500">Loading…</div>
        ) : entries.length === 0 ? (
          <div className="text-sm text-neutral-500">No price history available.</div>
        ) : (
          <ul className="divide-y divide-neutral-100">
            {entries.map((e, i) => (
              <li key={i} className="flex items-center justify-between py-2 text-sm">
                <span className="text-neutral-600">{new Date(e.date).toLocaleString()}</span>
                <span className="font-medium text-neutral-900">€{e.price}</span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

