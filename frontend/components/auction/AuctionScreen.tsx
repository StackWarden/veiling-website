// components/auction/AuctionScreen.tsx
"use client";

import { useEffect, useMemo, useRef, useState } from "react";

import ProductOverviewCard from "@/components/auction/ProductOverviewCard";
import BidPanel from "@/components/auction/BidPanel";
import NextProductCard from "@/components/auction/NextProductCard";

type LiveProduct = {
  id: string;
  title: string;
  photoUrl: string | null;
  species: string;
  stemLength: number;
  quantity: number;
  minPrice: number;
  potSize: string;
};

type LiveAuctionState = {
  auctionId: string;
  status: "running" | "stopped" | string;

  serverTimeUtc: string;

  roundIndex: number;
  maxRounds: number;
  roundStartedAtUtc: string;

  startingPrice: number;
  minPrice: number;
  decrementPerSecond: number;
  currentPrice: number;

  auctionItemId: string | null;
  product: LiveProduct | null;

  nextAuctionItemId: string | null;
};

type Props = {
  auctionId: string;
};

function parseUtcMs(iso: string) {
  const ms = Date.parse(iso);
  return Number.isFinite(ms) ? ms : 0;
}

export default function AuctionScreen({ auctionId }: Props) {
  const baseUrl = process.env.NEXT_PUBLIC_API_URL;

  const [live, setLive] = useState<LiveAuctionState | null>(null);
  const [displayPrice, setDisplayPrice] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // serverTime - clientNow (ms). Use it to make countdown consistent.
  const serverOffsetMsRef = useRef<number>(0);
  const rafRef = useRef<number | null>(null);
  const pollRef = useRef<number | null>(null);

  async function fetchLive(): Promise<LiveAuctionState> {
    const res = await fetch(`${baseUrl}/auctions/${auctionId}/live`, {
      method: "GET",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
    });

    if (!res.ok) {
      const text = await res.text().catch(() => "");
      throw new Error(text || `Failed to fetch live state (${res.status})`);
    }

    const data = (await res.json()) as LiveAuctionState;

    // compute offset
    if (data.serverTimeUtc) {
      const serverMs = parseUtcMs(data.serverTimeUtc);
      const clientMs = Date.now();
      if (serverMs > 0) serverOffsetMsRef.current = serverMs - clientMs;
    }

    return data;
  }

  // Initial load + polling
  useEffect(() => {
    let cancelled = false;

    async function loadOnce() {
      try {
        setIsLoading(true);
        setError(null);
        const data = await fetchLive();
        if (!cancelled) setLive(data);
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : "Unknown error");
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    loadOnce();

    pollRef.current = window.setInterval(() => {
      fetchLive()
        .then((data) => {
          if (!cancelled) setLive(data);
        })
        .catch(() => {
        });
    }, 2000);

    return () => {
      cancelled = true;
      if (pollRef.current) window.clearInterval(pollRef.current);
      pollRef.current = null;
    };
  }, [auctionId, baseUrl]);

  useEffect(() => {
    function tick() {
      if (live?.status !== "running" || !live.roundStartedAtUtc) {
        setDisplayPrice(live?.currentPrice ?? 0);
        rafRef.current = requestAnimationFrame(tick);
        return;
      }

      const nowMs = Date.now() + serverOffsetMsRef.current;
      const startedMs = parseUtcMs(live.roundStartedAtUtc);
      const elapsedSeconds = Math.max(0, (nowMs - startedMs) / 1000);

      const raw = live.startingPrice - elapsedSeconds * live.decrementPerSecond;
      const computed = Math.max(live.minPrice, raw);

      setDisplayPrice(computed);
      rafRef.current = requestAnimationFrame(tick);
    }

    rafRef.current = requestAnimationFrame(tick);
    return () => {
      if (rafRef.current) cancelAnimationFrame(rafRef.current);
      rafRef.current = null;
    };
  }, [live]);

  async function placeBid(quantity: number) {
    if (!live?.auctionItemId) return;

    const res = await fetch(`${baseUrl}/auctions/${auctionId}/live/bid`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        auctionItemId: live.auctionItemId,
        quantity,
      }),
    });

    if (!res.ok) {
      const text = await res.text().catch(() => "");
      throw new Error(text || `Failed to place bid (${res.status})`);
    }

    const data = (await res.json()) as { accepted: boolean; acceptedPrice: number; state: LiveAuctionState };
    // Update state immediately (don’t wait for poll)
    setLive(data.state);
  }

  const productCard = useMemo(() => {
    const p = live?.product;
    if (!p) return null;

    const imageUrl =
      p.photoUrl ??
      "https://images.unsplash.com/photo-1525310072745-f49212b5ac6d?auto=format&fit=crop&w=1200&q=80";

    return {
      title: p.title,
      imageUrl,
      extraInfo: [
        { label: "Species", value: p.species },
        { label: "Stem length (height)", value: `${p.stemLength}` },
        { label: "Quantity", value: `${p.quantity}` },
        { label: "Minimum price", value: `€${p.minPrice}` },
        { label: "Pot size", value: p.potSize },
      ],
    };
  }, [live]);

  const roundLabel = useMemo(() => {
    const idx = live?.roundIndex ?? 0;
    const max = live?.maxRounds ?? 3;
    return idx > 0 ? `Round ${idx}/${max}` : `Round -/${max}`;
  }, [live]);

  const showNext = Boolean(live?.nextAuctionItemId);
  const nextPlaceholder = {
    title: "Next item",
    imageUrl:
      "https://images.unsplash.com/photo-1545231097-cbd796f1d95d?auto=format&fit=crop&w=1200&q=80",
    species: "—",
    minimumPrice: "—",
    quantity: "—",
  };

  if (!isLoading && !error && !productCard) {
    return (
      <section className="flex flex-col items-center justify-center h-[calc(100vh-120px)] px-4 text-center space-y-6">
        <div className="text-xl font-semibold text-neutral-800">
          This auction hasn't started yet, or there are no items to display.
        </div>
        <a
          href="/auctions"
          className="inline-block rounded-lg bg-neutral-900 px-6 py-3 text-base font-semibold text-white hover:bg-neutral-800 transition"
        >
          Back to Auctions
        </a>
      </section>
    );
  }

  return (
    <section className="grid grid-cols-1 gap-6 lg:grid-cols-12">
      <div className="lg:col-span-8">
        {isLoading && (
          <div className="rounded-2xl border border-neutral-200 bg-white p-6 shadow-sm">
            Loading live auction...
          </div>
        )}

        {!isLoading && error && (
          <div className="rounded-2xl border border-red-200 bg-white p-6 text-sm text-red-700 shadow-sm">
            {error}
          </div>
        )}

        {!isLoading && !error && productCard && (
          <ProductOverviewCard
            title={productCard.title}
            imageUrl={productCard.imageUrl}
            extraInfo={productCard.extraInfo}
          />
        )}
      </div>

      {productCard && (
        <div className="lg:col-span-4">
          <div className="space-y-6">
            <BidPanel
              roundLabel={roundLabel}
              startingOffer={live?.startingPrice ?? 0}
              currentPrice={Number.isFinite(displayPrice) ? displayPrice : live?.currentPrice ?? 0}
              currency={"€"}
              onPlaceBid={(qty) => {
                placeBid(qty)
                  .then(() => console.log("Bid OK"))
                  .catch((e) => {
                    console.error("Bid failed:", e);
                  });
              }}
            />

            {showNext ? (
              <NextProductCard
                title={nextPlaceholder.title}
                imageUrl={nextPlaceholder.imageUrl}
                species={nextPlaceholder.species}
                minimumPrice={nextPlaceholder.minimumPrice}
                quantity={nextPlaceholder.quantity}
                onNext={() => {
                  console.log("Next product:", live?.nextAuctionItemId);
                }}
              />
            ) : null}
          </div>
        </div>
      )}
    </section>
  );

}
