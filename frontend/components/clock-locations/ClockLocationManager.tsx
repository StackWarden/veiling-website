"use client";

import { useEffect, useState } from "react";
import useGet from "../api/get";
import { usePost } from "../api/post";
import useDelete from "../api/delete";
import { RoleGate } from "../RoleGate";

type ClockLocation = {
  id: string;
  name: string;
};

type CreateClockLocationDto = {
  name: string;
};

type ClockLocationManagerProps = {
  onClockLocationChange?: () => void;
};

export default function ClockLocationManager({ onClockLocationChange }: ClockLocationManagerProps) {
  const [clockLocations, setClockLocations] = useState<ClockLocation[]>([]);
  const [newClockLocationName, setNewClockLocationName] = useState("");

  const { loading, execute: fetchClockLocations } = useGet<ClockLocation>({
    route: "/clock-locations",
    autoFetch: false,
    onSuccess: (data) => setClockLocations(data),
  });

  const {
    loading: creating,
    error: createError,
    execute: createClockLocation,
  } = usePost<CreateClockLocationDto, ClockLocation>({
    route: "/clock-locations",
    onSuccess: (data) => {
      // Clear input field
      setNewClockLocationName("");
      // Refresh the manager's list
      fetchClockLocations();
      // Notify parent component to refresh dropdown
      onClockLocationChange?.();
    },
    onError: (error) => {
      // Error is already displayed via createError state
      console.error("Failed to create clock location:", error);
    },
  });

  const { loading: deleting, execute: deleteClockLocation } = useDelete({
    baseRoute: "/clock-locations",
    onSuccess: () => {
      fetchClockLocations();
      onClockLocationChange?.();
    },
  });

  useEffect(() => {
    fetchClockLocations();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCreate = async (e?: React.FormEvent) => {
    if (e) {
      e.preventDefault();
      e.stopPropagation();
    }
    
    const trimmedName = newClockLocationName.trim();
    if (!trimmedName) {
      return;
    }
    
    try {
      const result = await createClockLocation({ name: trimmedName });
      // If creation was successful, the onSuccess callback will handle refresh
      // If it failed, the error will be displayed via createError state
    } catch (error) {
      // Error is already handled by usePost hook and displayed via createError
      // This catch is just for additional logging if needed
      console.error("Failed to create clock location:", error);
    }
  };


  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this clock location?")) return;
    await deleteClockLocation(id);
  };

  return (
    <RoleGate allow={["admin"]}>
      <div className="border rounded-lg p-6 bg-gray-50">
        <h2 className="text-xl font-bold mb-4">Manage Clock Locations</h2>

        {/* Create Form */}
        <form
          className="mb-6"
          onSubmit={async (e) => {
            e.preventDefault();
            e.stopPropagation();
            await handleCreate(e);
            return false;
          }}
        >
          <div className="flex gap-2">
            <input
              type="text"
              value={newClockLocationName}
              onChange={(e) => setNewClockLocationName(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  handleCreate();
                }
              }}
              placeholder="Clock location name"
              className="flex-1 border rounded-lg p-2"
              disabled={creating}
            />
            <button
              type="submit"
              disabled={creating || !newClockLocationName.trim()}
              className="bg-[#162218] text-white px-4 py-2 rounded-lg font-semibold hover:bg-[#0f1c14] disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {creating ? "Adding..." : "Add"}
            </button>
          </div>
          {createError && (
            <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded">
              <p className="text-red-600 text-sm font-medium">Error: {createError}</p>
            </div>
          )}
        </form>

        {/* List */}
        {loading || deleting ? (
          <p className="text-gray-500">Loading clock locations...</p>
        ) : clockLocations.length === 0 ? (
          <p className="text-gray-500">No clock locations available.</p>
        ) : (
          <div className="space-y-2">
            {clockLocations.map((cl) => (
              <div
                key={cl.id}
                className="flex items-center justify-between border rounded-lg p-3 bg-white"
              >
                <span className="font-medium">{cl.name}</span>
                <button
                  onClick={() => handleDelete(cl.id)}
                  className="text-red-600 hover:text-red-400 hover:underline"
                  type="button"
                >
                  Delete
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </RoleGate>
  );
}
