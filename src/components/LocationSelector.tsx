'use client';

import { useLocation } from '@/contexts/LocationContext';

export default function LocationSelector() {
  const { locations, selectedLocation, setSelectedLocation, isLoading, error } = useLocation();

  if (isLoading) return <div>Loading locations...</div>;
  if (error) return <div className="text-red-600">Error: {error}</div>;
  if (!locations.length) return <div>No locations available</div>;

  return (
    <div className="flex items-center gap-2">
      <label htmlFor="location" className="text-sm font-medium text-gray-700">
        Location:
      </label>
      <select
        id="location"
        value={selectedLocation?.id}
        onChange={(e) => {
          const location = locations.find(l => l.id === e.target.value);
          if (location) setSelectedLocation(location);
        }}
        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm"
      >
        {locations.map(location => (
          <option 
            key={location.id} 
            value={location.id}
            disabled={!location.isActive}
          >
            {location.name}
          </option>
        ))}
      </select>
    </div>
  );
}