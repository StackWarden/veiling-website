"use client";

import Header from "@/components/header";
import SpeciesForm from "@/components/species/speciesForm";
import useGet from "@/components/api/get";
import { Species } from "@/components/types/species";
import { useParams, useRouter } from "next/navigation";

export default function EditSpeciesPage() {
  const { id } = useParams();
  const router = useRouter();

  const { data, loading, error } = useGet<Species>({
    route: `/species/${id}`,
    transform: (payload) => [payload as Species],
  });

  return (
    <>
      <Header />

      <div className="max-w-3xl mx-auto mt-10">
        {loading && (
          <p className="text-center mt-10">Loading...</p>
        )}

        {error && (
          <p className="text-center text-red-600 mt-10">
            {error}
          </p>
        )}

        {!loading && !error && !data[0] && (
          <p className="text-center mt-10">
            Species not found
          </p>
        )}

        {!loading && !error && data[0] && (
          <SpeciesForm
            existing={data[0]}
            onSaved={() => router.push("/species")}
          />
        )}
      </div>
    </>
  );
}