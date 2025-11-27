"use client";

import { useState, useCallback } from "react";

/**
 * Opties voor de usePost-hook.
 */
interface UsePostOptions<TBody, TResult> {
  /** De API-route waarheen gepost moet worden (zonder ID) */
  route: string;

  /** Callback bij succes met teruggegeven data */
  onSuccess?: (data: TResult) => void;

  /** Callback bij fouten */
  onError?: (error: Error) => void;
}

/**
 * Returnwaarden van de usePost-hook.
 */
interface UsePostReturn<TBody, TResult> {
  loading: boolean;
  error: string;

  /** Voert POST uit met body */
  execute: (body: TBody) => Promise<TResult | null>;
}

/**
 * usePost
 * Herbruikbare hook voor geauthenticeerde POST-requests met JSON-body.
 */
export function usePost<TBody extends object, TResult = unknown>({
  route,
  onSuccess,
  onError,
}: UsePostOptions<TBody, TResult>): UsePostReturn<TBody, TResult> {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  /**
   * Voert de POST-aanvraag uit met JSON-body.
   */
  const execute = useCallback(
    async (body: TBody): Promise<TResult | null> => {
      setLoading(true);
      setError("");

      try {
        const response = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}${route}`,
          {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
          }
        );

        if (!response.ok) {
          throw new Error(`POST mislukt: ${response.statusText}`);
        }

        const data = (await response.json()) as TResult;

        if (onSuccess) onSuccess(data);

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
    [route, onSuccess, onError]
  );

  return { loading, error, execute };
}
