// components/auction/ExtraInfoList.tsx
export type ExtraInfoItem = {
  label: string;
  value: string;
};

type Props = {
  items: ExtraInfoItem[];
};

function Row({ label, value, isLast }: { label: string; value: string; isLast: boolean }) {
  return (
    <div className={`py-3 ${isLast ? "" : "border-b border-neutral-200"}`}>
      <div className="text-xs font-semibold uppercase tracking-wide text-neutral-700">{label}</div>
      <div className="mt-1 text-sm text-neutral-600">{value}</div>
    </div>
  );
}

export default function ExtraInfoList({ items }: Props) {
  return (
    <div className="divide-y-0">
      {items.map((it, idx) => (
        <Row key={`${it.label}-${idx}`} label={it.label} value={it.value} isLast={idx === items.length - 1} />
      ))}
    </div>
  );
}
