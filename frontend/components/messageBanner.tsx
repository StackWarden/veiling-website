"use client";

import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";

export default function MessageBanner() {
  const searchParams = useSearchParams();

  const successMessage = searchParams.get("success");
  const errorMessage = searchParams.get("message");

  const [message, setMessage] = useState("");
  const [type, setType] = useState<"success" | "error" | null>(null);
  const [visible, setVisible] = useState(true);

  useEffect(() => {
    if (successMessage) {
      setMessage(successMessage);
      setType("success");
      setVisible(true);
    } else if (errorMessage) {
      setMessage(errorMessage);
      setType("error");
      setVisible(true);
    } else {
      setMessage("");
      setType(null);
      setVisible(false);
    }
  }, [successMessage, errorMessage]);

  if (!message || !type || !visible) return null;

  return (
    <div
      className={`relative border px-4 py-3 text-center ${
        type === "success"
          ? "bg-green-100 border-green-300 text-gseen-700"
          : "bg-red-100 border-red-300 text-red-700"
      }`}
    >
      <button
        onClick={() => setVisible(false)}
        className="absolute right-4 top-1/2 -translate-y-1/2 text-xl"
        aria-label="Close message"
      >
        ×
      </button>

      {message}
    </div>
  );
}
