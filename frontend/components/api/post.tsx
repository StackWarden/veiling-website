"use client";

import { useState, useCallback } from "react";

/**
 * Opties voor de usePost-hook.
 */
interface UsePostOptions<TBody, TResult> {
  /** De API-route waarheen gepost moet worden (zonder ID) */
  route: string;

  /** Callback bij succes met teruggegeven data */
  onSuccess?: (data: TResult, body: TBody) => void;

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
        const apiUrl = `${process.env.NEXT_PUBLIC_API_URL}${route}`;
        // #region agent log
        fetch('http://127.0.0.1:7242/ingest/e43ac945-c26b-4b69-b531-933f97b6806d',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'post.tsx:51',message:'Making POST request',data:{apiUrl,route,envVar:process.env.NEXT_PUBLIC_API_URL,origin:window.location.origin},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'C'})}).catch(()=>{});
        // #endregion
        const response = await fetch(
          apiUrl,
          {
            method: "POST",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
          }
        );
        // #region agent log
        fetch('http://127.0.0.1:7242/ingest/e43ac945-c26b-4b69-b531-933f97b6806d',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'post.tsx:60',message:'POST response received',data:{status:response.status,statusText:response.statusText,ok:response.ok,headers:Object.fromEntries(response.headers.entries())},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'D'})}).catch(()=>{});
        // #endregion

        if (!response.ok) {
          let errorMessage = `POST mislukt: ${response.statusText}`;
          try {
            const errorData = await response.json() as 
              | { message?: string; errors?: Array<{ description?: string; code?: string }> }
              | string
              | Array<{ description?: string; code?: string }>;
            
            if (typeof errorData === 'object' && errorData !== null && !Array.isArray(errorData)) {
              if (errorData.message) {
                errorMessage = errorData.message;
              } else if (errorData.errors && Array.isArray(errorData.errors)) {
                const errorMessages = errorData.errors.map((err) => 
                  err.description || err.code || JSON.stringify(err)
                ).join(', ');
                errorMessage = errorMessages || errorMessage;
              }
            } else if (typeof errorData === 'string') {
              errorMessage = errorData;
            } else if (Array.isArray(errorData)) {
              errorMessage = errorData.map((err) => 
                err.description || err.code || JSON.stringify(err)
              ).join(', ');
            }
          } catch {
          }
          throw new Error(errorMessage);
        }

        const data = (await response.json()) as TResult;

        if (onSuccess) onSuccess(data, body);

        return data;
      } catch (err) {
        const errorObj = err instanceof Error ? err : new Error("Onbekende fout");
        // #region agent log
        fetch('http://127.0.0.1:7242/ingest/e43ac945-c26b-4b69-b531-933f97b6806d',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'post.tsx:catch',message:'POST request failed',data:{error:errorObj.message,stack:errorObj.stack,name:errorObj.name},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'D'})}).catch(()=>{});
        // #endregion
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
