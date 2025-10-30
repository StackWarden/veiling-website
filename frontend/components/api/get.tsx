"use client";

import { useCallback, useEffect, useState } from "react";

interface UseGetOptions<TData> {
  route: string;
  autoFetch?: boolean;
  params?: Record<string, string | number | boolean | null | undefined>;
  onSuccess?: (data: TData[]) => void;
  onError?: (error: Error) => void;
  transform?: (payload: unknown) => TData[];
}

interface UseGetReturn<TData> {
  data: TData[];
  loading: boolean;
  error: string;
  refetch: () => Promise<void>;
  setData: React.Dispatch<React.SetStateAction<TData[]>>;
}

const buildUrl = (route: string, params?: UseGetOptions<unknown>["params"]) => {
  const url = new URL(`${process.env.NEXT_PUBLIC_API_URL}${route}`);

  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value === undefined || value === null) return;
      url.searchParams.append(key, String(value));
    });
  }

  return url.toString();
};

export default function useGet<TData = unknown>({
  route,
  autoFetch = true,
  params,
  onSuccess,
  onError,
  transform,
}: UseGetOptions<TData>): UseGetReturn<TData> {
  const [data, setData] = useState<TData[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const refetch = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const response = await fetch(buildUrl(route, params));

      if (!response.ok) throw new Error(`Failed to fetch ${route}`);

      const payload = await response.json();
      const parsedData = transform ? transform(payload) : (payload as TData[]);

      setData(parsedData);
      if (onSuccess) onSuccess(parsedData);
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
        if (onError) onError(err);
      } else {
        const unknownError = new Error("An unknown error occurred");
        setError(unknownError.message);
        if (onError) onError(unknownError);
      }
    } finally {
      setLoading(false);
    }
  }, [route, params, transform, onSuccess, onError]);

  useEffect(() => {
    if (autoFetch) void refetch();
  }, [autoFetch, refetch]);

  return { data, loading, error, refetch, setData };
}
