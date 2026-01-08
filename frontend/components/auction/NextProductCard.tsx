// components/auction/NextProductCard.tsx
"use client";

import { useState, useEffect } from "react";

type Props = {
  title: string;
  imageUrl: string;
  species: string;
  minimumPrice: string;
  quantity: string;
  onNext: () => void;
};

function StatHeader({ label }: { label: string }) {
  return <div className="text-[11px] font-semibold uppercase tracking-wide text-neutral-200">{label}</div>;
}

function StatCell({ value }: { value: string }) {
  return <div className="text-sm font-semibold text-neutral-900">{value}</div>;
}

export default function NextProductCard({ title, imageUrl, species, minimumPrice, quantity, onNext }: Props) {
  const [imgError, setImgError] = useState(false);
  const displayImageUrl = imgError || !imageUrl ? "/images/placeholder.jpg" : imageUrl;

  // Reset error state when imageUrl changes
  useEffect(() => {
    setImgError(false);
  }, [imageUrl]);

  return (
    <>
        <div className="rounded-2xl border border-neutral-200 bg-white p-6 shadow-sm">
        <h3 className="text-lg font-semibold text-neutral-900">{title}</h3>

        <div className="mt-2 overflow-hidden rounded-2xl bg-neutral-100">
            <div className="aspect-[16/10] w-full">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img 
              src={displayImageUrl} 
              alt={title} 
              className="h-full w-full object-cover"
              onError={() => setImgError(true)}
            />
            </div>
        </div>

        <div className="mt-1 overflow-hidden rounded-xl border border-neutral-200">
            <div className="grid grid-cols-3 bg-neutral-900 px-3 py-2">
            <StatHeader label="Species" />
            <div className="text-center">
                <StatHeader label="Minimum price" />
            </div>
            <div className="text-right">
                <StatHeader label="Quantity" />
            </div>
            </div>
            <div className="grid grid-cols-3 px-3 py-3">
            <StatCell value={species} />
            <div className="text-center">
                <StatCell value={minimumPrice} />
            </div>
            <div className="text-right">
                <StatCell value={quantity} />
            </div>
            </div>
        </div>

        </div>
        
        <div className="mt-5 flex items-center justify-end gap-3">
            <button type="button" onClick={onNext} className="text-xs font-semibold text-neutral-700 hover:text-neutral-900">
            See next product
            </button>

            <button
            type="button"
            onClick={onNext}
            aria-label="Next product"
            className="grid h-10 w-10 place-items-center rounded-full bg-neutral-900 text-white hover:bg-neutral-800 active:bg-neutral-950"
            >
            <span className="text-lg leading-none">›</span>
            </button>
        </div>
    </>
  );
}
