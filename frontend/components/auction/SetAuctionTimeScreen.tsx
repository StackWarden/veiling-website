"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";

import Form, { FormField } from "@/components/form";
import useGet from "@/components/api/get";

type Auction = {
  id: string;
  auctionDate: string; // "YYYY-MM-DD"
  auctionTime: string | null; // "HH:mm" or null
};

type SetTimePayload = {
  auctionTime: string | null;
};

export default function SetAuctionTimeScreen({ auctionId }: { auctionId: string }) {
  const router = useRouter();

  const [auction, setAuction] = useState<Auction | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  // Form state moet bovenaan, niet na een return
  const [form, setForm] = useState<SetTimePayload>({ auctionTime: null });

  const { execute: fetchAuction, loading: loadingAuction } = useGet<Auction>({
    route: `/auctions/${auctionId}`,
    autoFetch: false,
    onSuccess: (data) => {
        const maybeArray = data as unknown as Auction | Auction[];
        const one = Array.isArray(maybeArray) ? (maybeArray[0] ?? null) : maybeArray;
        setAuction(one);
    },
  });

  useEffect(() => {
    fetchAuction();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Zodra auction geladen is, zet je init value in form (en blijf in sync)
  useEffect(() => {
    if (!auction) return;
    setForm({ auctionTime: auction.auctionTime });
  }, [auction]);

  const fields: Array<FormField<SetTimePayload>> = useMemo(
    () => [
      {
        name: "auctionTime",
        label: "Auction Time",
        type: "time",
        formatValue: (v) => (v ? v : ""),
        parseValue: (raw) => (raw && raw.length > 0 ? raw : null),
      },
    ],
    []
  );

  const patchAuctionTime = async (payload: SetTimePayload) => {
    setSaving(true);
    setError("");

    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/auctions/${auctionId}/time`,
        {
          method: "PATCH",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        }
      );

      if (!response.ok) {
        const text = await response.text().catch(() => "");
        throw new Error(text || `PATCH failed: ${response.status} ${response.statusText}`);
      }

      router.push(`/auctions`);
    } catch (err) {
      const errorObj = err instanceof Error ? err : new Error("Unknown error");
      setError(errorObj.message);
    } finally {
      setSaving(false);
    }
  };

  if (loadingAuction || !auction) {
    return <p className="text-center py-12">Loading auction…</p>;
  }

  return (
    <Form<SetTimePayload>
      title={`Set auction time (${auction.auctionDate})`}
      values={form}
      setValues={setForm}
      fields={fields}
      columns={1}
      submitting={saving}
      error={error}
      submitLabel="Save time"
      onSubmit={async (values) => {
        await patchAuctionTime(values);
      }}
    />
  );
}
