"use client";

import { useState, useCallback } from "react";

/**
 * Opties voor de usePut-hook.
 */
interface UsePutOptions<TBody, TResult> {
  /** De basisroute zonder ID, bv: "/auctions" */
  baseRoute: string;

  /** Callback bij succes */
  onSuccess?: (data: TResult, id: string, body: TBody) => void;

  /** Callback bij fout */
  onError?: (error: Error) => void;
}

/**
 * Returnwaarden van de usePut-hook.
 */
interface UsePutReturn<TBody, TResult> {
  loading: boolean;
  error: string;

  /** Voert een PUT uit naar `${baseRoute}/${id}` met body */
  execute: (id: string, body: TBody) => Promise<TResult | null>;
}

/**
 * usePut
 * Herbruikbare hook voor geauthenticeerde PUT-requests.
 */
export function usePut<TBody extends object, TResult = unknown>({
  baseRoute,
  onSuccess,
  onError,
}: UsePutOptions<TBody, TResult>): UsePutReturn<TBody, TResult> {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const execute = useCallback(
    async (id: string, body: TBody): Promise<TResult | null> => {
      setLoading(true);
      setError("");

      try {
        const response = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}${baseRoute}/${id}`,
          {
            method: "PUT",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
          }
        );

        if (!response.ok) throw new Error(`PUT mislukt: ${response.statusText}`);

        const data = (await response.json()) as TResult;

        if (onSuccess) onSuccess(data, id, body);

        return data;
      } catch (err) {
        const errorObj = err instanceof Error ? err : new Error("Onbekende fout");

        setError(errorObj.message);
        if (onError) onError(errorObj);

        return null;
      } finally {
        setLoading(false);
      }
    },
    [baseRoute, onSuccess, onError]
  );

  return { loading, error, execute };
}
