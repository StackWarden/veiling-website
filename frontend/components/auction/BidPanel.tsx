"use client";

import { useMemo, useState } from "react";

type Props = {
  roundLabel: string;
  startingOffer: number;
  currentPrice: number;
  minPrice: number;
  showMinPrice: boolean;
  currency?: string;
  onPlaceBid: (quantity: number) => void;
};

function formatMoney(currency: string, value: number) {
  return `${currency}${value.toFixed(2)}`;
}

export default function BidPanel({
  roundLabel,
  startingOffer,
  currentPrice,
  minPrice,
  showMinPrice,
  currency = "€",
  onPlaceBid,
}: Props) {
  const [quantity, setQuantity] = useState<string>("");

  const progress = useMemo(() => {
    if (startingOffer <= 0) return 0;
    const clamped = Math.max(0, Math.min(1, currentPrice / startingOffer));
    return Math.round(clamped * 100);
  }, [startingOffer, currentPrice]);

  function submit() {
    const parsed = Number(quantity);

    if (!Number.isFinite(parsed) || parsed <= 0) {
      return;
    }

    onPlaceBid(parsed);
  }

  return (
    <div className="rounded-2xl border border-neutral-200 bg-white p-6 shadow-sm">
      <div className="text-right text-sm font-semibold text-neutral-900">{roundLabel}</div>

      <div className="mt-4 grid grid-cols-2 gap-3 text-xs">
        <div className="text-neutral-600">
          <div className="text-[11px] font-semibold uppercase tracking-wide text-neutral-500">Starting offer</div>
          <div className="mt-1 text-sm font-semibold text-neutral-900">{formatMoney(currency, startingOffer)}</div>
        </div>

        <div className="text-right text-neutral-600">
          <div className="text-[11px] font-semibold uppercase tracking-wide text-neutral-500">Current price</div>
          <div className="mt-1 text-sm font-semibold text-neutral-900">{formatMoney(currency, currentPrice)}</div>
        </div>
      </div>

      {showMinPrice ? (
        <div className="mt-3 text-xs text-neutral-600">
          <div className="text-[11px] font-semibold uppercase tracking-wide text-neutral-500">Current minimum</div>
          <div className="mt-1 text-sm font-semibold text-neutral-900">{formatMoney(currency, minPrice)}</div>
        </div>
      ) : null}

      <div className="mt-3">
        <div className="h-2 w-full overflow-hidden rounded-full bg-neutral-200">
          <div className="h-full rounded-full bg-neutral-900" style={{ width: `${progress}%` }} />
        </div>
      </div>

      <div className="mt-4">
        <label className="sr-only" htmlFor="bid-qty">
          Quantity
        </label>
        <input
          type="number"
          min={1}
          step={1}
          id="bid-qty"
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
          placeholder="Quantity..."
          className="w-full rounded-xl border border-neutral-200 bg-white px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-neutral-300"
          inputMode="numeric"
          required
        />
      </div>

      <button
        type="button"
        onClick={submit}
        className="mt-4 w-full rounded-xl bg-neutral-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-neutral-800 active:bg-neutral-950"
      >
        Place your bid!
      </button>
    </div>
  );
}
