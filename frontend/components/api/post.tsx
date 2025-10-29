"use client";

import React, { useState } from "react";

interface PostApiProps<T> {
  route: string;
  title?: string;
  initialData: T;
}

export default function PostApi<T extends Record<string, any>>({
  route,
  title,
  initialData,
}: PostApiProps<T>) {
  const [formData, setFormData] = useState<T>(initialData);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const handleChange = (key: keyof T, value: any) => {
    setFormData((prev) => ({ ...prev, [key]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");
    setSuccess(false);

    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${route}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData),
      });

      if (!response.ok) throw new Error(`Failed post to ${route}`);
      setSuccess(true);
      setFormData(initialData);
    } catch (err) {
      if (err instanceof Error) setError(err.message);
      else setError("unknown error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: "2rem" }}>
      <h1>Create {title ?? "Item"}</h1>

      <form onSubmit={handleSubmit}>
        {Object.entries(formData).map(([key, value]) => (
          <div key={key} style={{ marginBottom: "1rem" }}>
            <label>
              {key}:{" "}
              <input
                type="text"
                value={value as any}
                onChange={(e) => handleChange(key as keyof T, e.target.value)}
                style={{ padding: "0.3rem", width: "300px" }}
              />
            </label>
          </div>
        ))}

        <button type="submit" disabled={loading}>
          {loading ? "Posting..." : `Post ${title ?? "Item"}`}
        </button>
      </form>

      {error && <p style={{ color: "red" }}>Nope: {error}</p>}
      {success && <p style={{ color: "green" }}>Success :D</p>}
    </div>
  );
}
