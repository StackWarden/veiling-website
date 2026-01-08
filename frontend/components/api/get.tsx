"use client";

import { useCallback, useEffect, useRef, useState } from "react";

/**
 * Opties voor de useGet-hook.
 */
interface UseGetOptions<TData> {
  /** De API-route, bv: "/products" */
  route: string;

  /** Automatisch ophalen bij laden (default: true) */
  autoFetch?: boolean;

  /** Queryparameters als key/value */
  params?: Record<string, string | number | boolean | null | undefined>;

  /** Callback bij succesvol ophalen */
  onSuccess?: (data: TData[]) => void;

  /** Callback bij fouten */
  onError?: (error: Error) => void;

  /** Optionele transformatie van de response */
  transform?: (payload: unknown) => TData[];
}

/**
 * Wat de useGet-hook teruggeeft.
 */
interface UseGetReturn<TData> {
  data: TData[];
  loading: boolean;
  error: string;

  /** Handmatig opnieuw data ophalen */
  execute: () => Promise<void>;

  /** Direct data aanpassen in state */
  setData: React.Dispatch<React.SetStateAction<TData[]>>;
}

/**
 * Bouwt een volledige URL inclusief queryparameters.
 */
const buildUrl = (
  route: string,
  params?: UseGetOptions<unknown>["params"]
) => {
  const url = new URL(`${process.env.NEXT_PUBLIC_API_URL}${route}`);

  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value !== null && value !== undefined) {
        url.searchParams.append(key, String(value));
      }
    });
  }

  return url.toString();
};

/**
 * useGet
 * Herbruikbare hook voor het uitvoeren van geauthenticeerde GET-requests.
 * Ondersteunt automatische laadtijd, queryparameters en transformaties.
 */
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
  const hasFetchedRef = useRef(false);

  const onSuccessRef = useRef(onSuccess);
  const onErrorRef = useRef(onError);
  const transformRef = useRef(transform);
  
  useEffect(() => {
    onSuccessRef.current = onSuccess;
    onErrorRef.current = onError;
    transformRef.current = transform;
  }, [onSuccess, onError, transform]);

  /**
   * Voert de GET-aanvraag uit.
   */
  const execute = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      const response = await fetch(buildUrl(route, params), {
        method: "GET",
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error(`Fout bij ophalen: ${route}`);
      }

      const payload = await response.json();
      const parsed = transformRef.current ? transformRef.current(payload) : (payload as TData[]);

      setData(parsed);

      if (onSuccessRef.current) onSuccessRef.current(parsed);
    } catch (err) {
      const errorObj = err instanceof Error ? err : new Error("Onbekende fout");
      setError(errorObj.message);

      if (onErrorRef.current) onErrorRef.current(errorObj);
    } finally {
      setLoading(false);
    }
  }, [route, params]);

  /**
   * Automatisch uitvoeren bij mount of wanneer afhankelijkheden wijzigen.
   */
  useEffect(() => {
    if (!autoFetch) return;
    if (hasFetchedRef.current) return;

    hasFetchedRef.current = true;
    void execute();
  }, [autoFetch, execute]);

  return { data, loading, error, execute, setData };
}
