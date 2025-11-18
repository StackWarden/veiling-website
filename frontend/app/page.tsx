"use client";

import { useEffect } from "react";

export default function Home() {
  useEffect(() => {
    const token = localStorage.getItem("jwt");

    if (token) {
      window.location.href = "/auctions";
    }
    
    if (!token) {
      window.location.href = "/login";
    }
  }, []);

  return null;
}
