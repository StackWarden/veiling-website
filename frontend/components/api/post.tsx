"use client";

import { useState } from "react";

interface PostDataResult<T> {
  loading: boolean;
  error: string;
  success: boolean;
  postData: (data: T) => Promise<void>;
}

export function usePostData<T extends object>(
  route: string
): PostDataResult<T> {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const postData = async (data: T) => {
    setLoading(true);
    setError("");
    setSuccess(false);

    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${route}`, {
        credentials: "include",
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const text = await response.text();
        throw new Error(text || `Failed to POST to ${route}`);
      }

      setSuccess(true);
    } catch (err) {
      if (err instanceof Error) setError(err.message);
      else setError("Unknown error");
    } finally {
      setLoading(false);
    }
  };

  return { loading, error, success, postData };
}