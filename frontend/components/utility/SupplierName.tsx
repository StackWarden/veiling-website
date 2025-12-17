import useGet from "../api/get";

export function SupplierName({ supplierId }: { supplierId: string }) {
  const { data, loading } = useGet<{ name: string }>({
    route: `/auth/name/${supplierId}`,
    transform: (payload) => [payload as { name: string }],
  });

  if (loading) return <span>Loading...</span>;

  return <span>{data[0]?.name ?? "—"}</span>;
}
