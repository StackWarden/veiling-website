"use client";

import { useState, useCallback } from "react";

/**
 * Opties voor de useDelete-hook.
 */
interface UseDeleteOptions {
  /**
   * De basisroute waar DELETE-verzoeken naartoe gaan.
   */
  baseRoute: string;

  /**
   * Callback die uitgevoerd wordt wanneer verwijderen succesvol is.
   * Ontvangt het ID dat werd verwijderd.
   */
  onSuccess?: (id: string) => void;

  /**
   * Callback voor foutafhandeling.
   */
  onError?: (error: Error) => void;
}

/**
 * Returnwaarden van de useDelete-hook.
 */
interface UseDeleteReturn {
  loading: boolean;
  error: string;

  /**
   * Voert een DELETE-request uit naar `${baseRoute}/${id}`.
   */
  execute: (id: string) => Promise<void>;
}

/**
 * useDelete
 * Herbruikbare hook voor geauthenticeerde DELETE-requests.
 * Ondersteunt dynamische IDs, loading- en foutstatus,
 * en maakt gebruik van JWT-cookies via `credentials: include`.
 */
export default function useDelete({
  baseRoute,
  onSuccess,
  onError,
}: UseDeleteOptions): UseDeleteReturn {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  /**
   * Voert het DELETE-verzoek uit met een dynamisch ID.
   */
  const execute = useCallback(
    async (id: string) => {
      setLoading(true);
      setError("");

      try {
        const response = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}${baseRoute}/${id}`,
          {
            method: "DELETE",
            credentials: "include",
          }
        );

        if (!response.ok) {
          let errorMessage = `Verwijderen mislukt: ${response.statusText}`;
          try {
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.includes("application/json")) {
              const errorData = await response.json() as 
                | { message?: string; error?: string }
                | string;
              
              if (typeof errorData === 'object' && errorData !== null) {
                errorMessage = errorData.message || errorData.error || errorMessage;
              } else if (typeof errorData === 'string') {
                errorMessage = errorData;
              }
            } else {
              const text = await response.text();
              if (text) {
                errorMessage = text;
              }
            }
          } catch {
          }
          throw new Error(errorMessage);
        }

        if (onSuccess) onSuccess(id);
      } catch (err) {
        const errorObj = err instanceof Error ? err : new Error("Onbekende fout");
        setError(errorObj.message);

        if (onError) onError(errorObj);
      } finally {
        setLoading(false);
      }
    },
    [baseRoute, onSuccess, onError]
  );

  return { loading, error, execute };
}
