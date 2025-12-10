"use client";

import { ReactNode } from "react";

export interface ListHeader {
  key: string;
  label: string;
  align?: "start" | "center" | "end";
}

interface ListProps<T extends Record<string, unknown>> {
  headers: ListHeader[];
  rows: T[];
  rowKey: keyof T;
  actions?: ReactNode;
  onRowClick?: (row: T) => void;
}

export default function List<T extends Record<string, unknown>>({
  headers,
  rows,
  rowKey,
  actions,
  onRowClick,
}: ListProps<T>) {
   return (
    <>
      {/* Actions top right */}
      <div className="flex items-center mb-6 w-full pt-8 pb-4">
        <div className="flex-1" />
        {actions ? (
          <div className="flex-1 flex justify-end">{actions}</div>
        ) : (
          <div className="flex-1" />
        )}
      </div>

      <div className="overflow-hidden rounded-xl border border-[D9D9D9] p-4">
        <table className="w-full border-collapse text-left">
          <thead className="bg-white">
            <tr className="text-[#4D4D4D]">
              {headers.map((h) => (
                <th key={h.key} className={`p-3 text-${h.align ?? "start"}`}>
                  {h.label}
                </th>
              ))}
            </tr>
          </thead>

          <tbody className="bg-white text-[1A1A1A]">
            {rows.map((row) => (
              <tr
                key={String(row[rowKey])}
                className="hover:bg-[#162218] hover:text-white transition cursor-pointer"
                onClick={() => onRowClick?.(row)}
              >
                {headers.map((h) => (
                  <td
                    key={h.key}
                    className={`p-4 text-${h.align ?? "left"}`}
                  >
                    {row[h.key] as ReactNode}
                  </td>
                ))}

                { <td className="p-4 text-end">{row.actions as ReactNode}</td> }
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
