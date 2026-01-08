// components/auction/ProductOverviewCard.tsx
import ExtraInfoList, { ExtraInfoItem } from "@/components/auction/ExtraInfoList";

type Props = {
  title: string;
  imageUrl: string;
  extraInfo: ExtraInfoItem[];
};

export default function ProductOverviewCard({ title, imageUrl, extraInfo }: Props) {
  return (
    <div className="rounded-2xl border border-neutral-200 bg-white p-6 shadow-sm">
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="min-w-0">
          <h2 className="text-xl font-semibold text-neutral-900">{title}</h2>

          <div className="mt-4 overflow-hidden rounded-2xl bg-neutral-100">
            <div className="aspect-[4/5] w-full">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={imageUrl} alt={title} className="h-full w-full object-cover" />
            </div>
          </div>
        </div>

        <div className="min-w-0">
          <h3 className="text-lg font-semibold text-neutral-900">Extra information</h3>
          <div className="mt-3">
            <ExtraInfoList items={extraInfo} />
          </div>
        </div>
      </div>
    </div>
  );
}
