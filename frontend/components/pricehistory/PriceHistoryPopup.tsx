"use client";

import { useEffect, useMemo, useState } from "react";
import List from "@/components/list";

type PricePoint = {
  price: number;
  date: string; // ISO
  supplierId?: string | null;
};

type PriceHistory = {
  avgSupplier: number | null;
  avgOverall: number | null;
  last10Supplier: PricePoint[];
  last10Overall: PricePoint[];
};

type Row = {
  id: string;
  date: string;
  price: string;
  supplier?: string;
};

function formatMoney(v: number) {
  return new Intl.NumberFormat("nl-NL", {
    style: "currency",
    currency: "EUR",
    maximumFractionDigits: 2,
  }).format(v);
}

function formatDateTime(iso: string) {
  return new Date(iso).toLocaleString("nl-NL");
}

function clamp(n: number, min: number, max: number) {
  return Math.max(min, Math.min(max, n));
}

export default function PriceHistoryPopup({
  open,
  onClose,
  productId,
  title = "Verkoop geschiedenis",
}: {
  open: boolean;
  onClose: () => void;
  productId: string;
  title?: string;
}) {
  const [screen, setScreen] = useState(0);

  const [data, setData] = useState<PriceHistory | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!open) return;
    setScreen(0);
    setError("");
    setData(null);
  }, [open, productId]);

  useEffect(() => {
    if (!open) return;

    async function load() {
      setLoading(true);
      setError("");

      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/products/${productId}/price-history`,
          {
            method: "GET",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
          }
        );

        if (!res.ok) {
          const text = await res.text().catch(() => "");
          throw new Error(text || `Failed to load price history (${res.status})`);
        }

        const json = (await res.json()) as PriceHistory;
        setData(json);
      } catch (e) {
        const err = e instanceof Error ? e : new Error("Unknown error");
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [open, productId]);

  const supplierRows: Row[] = useMemo(() => {
    if (!data) return [];
    return (data.last10Supplier ?? []).map((p, idx) => ({
      id: `${idx}-${p.date}`,
      date: formatDateTime(p.date),
      price: formatMoney(p.price),
    }));
  }, [data]);

  const overallRows: Row[] = useMemo(() => {
    if (!data) return [];
    return (data.last10Overall ?? []).map((p, idx) => ({
      id: `${idx}-${p.date}-${p.supplierId ?? "none"}`,
      date: formatDateTime(p.date),
      price: formatMoney(p.price),
      supplier: p.supplierId ?? "-",
    }));
  }, [data]);

  const totalScreens = 3;
  const goLeft = () => setScreen((s) => clamp(s - 1, 0, totalScreens - 1));
  const goRight = () => setScreen((s) => clamp(s + 1, 0, totalScreens - 1));

  useEffect(() => {
    if (!open) return;

    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
      if (e.key === "ArrowLeft") goLeft();
      if (e.key === "ArrowRight") goRight();
    };

    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-[9999] flex items-center justify-center" role="dialog" aria-modal="true">
      {/* Backdrop */}
      <button
        type="button"
        onClick={onClose}
        className="absolute inset-0 bg-black/50"
        aria-label="Close price history"
      />

      {/* Modal */}
      <div className="relative w-[min(1100px,92vw)] max-h-[88vh] overflow-hidden rounded-2xl bg-white shadow-2xl border border-[#D9D9D9]">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-[#E5E5E5] bg-white">
          <div>
            <h2 className="text-xl font-bold text-[#162218]">{title}</h2>
            <p className="text-sm text-gray-500">
              Screen {screen + 1} / {totalScreens}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="px-3 py-2 rounded-lg hover:bg-gray-100 text-[#162218] font-semibold"
          >
            Close
          </button>
        </div>

        {/* Body */}
        <div className="px-6 py-6 overflow-auto max-h-[calc(88vh-120px)]">
          {loading && <p className="text-gray-500 text-center py-10">Loading price history...</p>}

          {!loading && error && <p className="text-red-600 text-center py-6 font-semibold">{error}</p>}

          {!loading && !error && data && (
            <>
              {screen === 0 && (
                <div className="space-y-6">
                  <h3 className="text-2xl font-bold text-[#162218]">Overzicht</h3>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="rounded-2xl border border-[#D9D9D9] bg-white p-6 shadow-sm">
                      <p className="text-sm text-gray-500">Gemiddelde prijs (huidige aanvoerder)</p>
                      <p className="text-3xl font-bold text-[#162218] mt-2">
                        {data.avgSupplier == null ? "-" : formatMoney(data.avgSupplier)}
                      </p>
                    </div>

                    <div className="rounded-2xl border border-[#D9D9D9] bg-white p-6 shadow-sm">
                      <p className="text-sm text-gray-500">Gemiddelde prijs (alle aanvoerders)</p>
                      <p className="text-3xl font-bold text-[#162218] mt-2">
                        {data.avgOverall == null ? "-" : formatMoney(data.avgOverall)}
                      </p>
                    </div>
                  </div>

                  <div className="rounded-xl bg-[#0f1c14] text-white p-4">
                    <p className="text-sm">
                      Deze informatie is onafhankelijk van het biedproces en heeft geen invloed op je bod.
                    </p>
                  </div>
                </div>
              )}

              {screen === 1 && (
                <div className="space-y-4">
                  <h3 className="text-2xl font-bold text-[#162218]">Laatste 10 prijzen (huidige aanvoerder)</h3>

                  <List<Row>
                    headers={[
                      { key: "date", label: "Datum", align: "start" },
                      { key: "price", label: "Prijs", align: "end" },
                    ]}
                    rows={supplierRows}
                    rowKey="id"
                  />
                </div>
              )}

              {screen === 2 && (
                <div className="space-y-4">
                  <h3 className="text-2xl font-bold text-[#162218]">Laatste 10 prijzen (alle aanvoerders)</h3>

                  <List<Row>
                    headers={[
                      { key: "date", label: "Datum", align: "start" },
                      { key: "price", label: "Prijs", align: "center" },
                      { key: "supplier", label: "Aanvoerder", align: "end" },
                    ]}
                    rows={overallRows}
                    rowKey="id"
                  />
                </div>
              )}
            </>
          )}
        </div>

        {/* Footer nav */}
        <div className="flex items-center justify-between px-6 py-4 border-t border-[#E5E5E5] bg-white">
          <button
            type="button"
            onClick={goLeft}
            disabled={screen === 0}
            className="px-4 py-2 rounded-lg border border-[#D9D9D9] font-semibold disabled:opacity-50 hover:bg-gray-50"
          >
            ◀ Previous
          </button>

          <div className="flex items-center gap-2">
            {[0, 1, 2].map((i) => (
              <button
                key={i}
                type="button"
                onClick={() => setScreen(i)}
                className={`h-2.5 w-2.5 rounded-full ${screen === i ? "bg-[#162218]" : "bg-[#D9D9D9]"}`}
                aria-label={`Go to screen ${i + 1}`}
              />
            ))}
          </div>

          <button
            type="button"
            onClick={goRight}
            disabled={screen === totalScreens - 1}
            className="px-4 py-2 rounded-lg border border-[#D9D9D9] font-semibold disabled:opacity-50 hover:bg-gray-50"
          >
            Next ▶
          </button>
        </div>
      </div>
    </div>
  );
}
