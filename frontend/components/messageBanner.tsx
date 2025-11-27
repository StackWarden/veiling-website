"use client";

import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";

export default function MessageBanner() {
  const searchParams = useSearchParams();
  const messageFromUrl = searchParams.get("message") || "";
  const [message, setMessage] = useState("");

  // Sync bij URL-verandering
  useEffect(() => {
    if (messageFromUrl) {
      setMessage(messageFromUrl);
    }
  }, [messageFromUrl]);

  if (!message) return null;

  return (
    <div className="bg-red-100 border border-red-300 text-red-700 px-4 py-3 text-center">
      {message}
    </div>
  );
}